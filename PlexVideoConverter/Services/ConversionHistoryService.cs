using System.Globalization;
using Microsoft.Data.Sqlite;
using PlexVideoConverter.Models;

namespace PlexVideoConverter.Services;

public sealed class ConversionHistoryService
{
    private readonly string connectionString;

    public ConversionHistoryService(string databasePath)
    {
        var fullPath = Path.GetFullPath(databasePath);
        Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);
        connectionString = new SqliteConnectionStringBuilder { DataSource = fullPath }.ToString();
        using var connection = OpenConnection();
        using var command = connection.CreateCommand();
        command.CommandText = """
            PRAGMA journal_mode = WAL;
            CREATE TABLE IF NOT EXISTS ConversionHistory (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                ConversionId TEXT NOT NULL UNIQUE,
                OriginalSize INTEGER NOT NULL CHECK (OriginalSize >= 0),
                ConvertedSize INTEGER NOT NULL CHECK (ConvertedSize >= 0),
                InputFileName TEXT NOT NULL,
                OutputFileName TEXT NOT NULL,
                InputPath TEXT NOT NULL,
                OutputPath TEXT NOT NULL,
                DateTimeConverted TEXT NOT NULL,
                PercentSizeReduction REAL NOT NULL,
                Crf INTEGER NOT NULL,
                VideoCodec TEXT NOT NULL,
                DurationSeconds REAL NOT NULL
            );
            """;
        command.ExecuteNonQuery();
    }

    public void Record(ConversionHistoryResponse conversion)
    {
        using var connection = OpenConnection();
        using var command = connection.CreateCommand();
        command.CommandText = """
            INSERT INTO ConversionHistory
                (ConversionId, OriginalSize, ConvertedSize, InputFileName, OutputFileName,
                 InputPath, OutputPath, DateTimeConverted, PercentSizeReduction, Crf, VideoCodec, DurationSeconds)
            VALUES ($id, $original, $converted, $inputName, $outputName, $inputPath, $outputPath,
                    $completed, $reduction, $crf, $codec, $duration)
            ON CONFLICT(ConversionId) DO NOTHING;
            """;
        command.Parameters.AddWithValue("$id", conversion.ConversionId.ToString());
        command.Parameters.AddWithValue("$original", conversion.OriginalSize);
        command.Parameters.AddWithValue("$converted", conversion.ConvertedSize);
        command.Parameters.AddWithValue("$inputName", conversion.InputFileName);
        command.Parameters.AddWithValue("$outputName", conversion.OutputFileName);
        command.Parameters.AddWithValue("$inputPath", conversion.InputPath);
        command.Parameters.AddWithValue("$outputPath", conversion.OutputPath);
        command.Parameters.AddWithValue("$completed", conversion.DateTimeConverted.ToUniversalTime().ToString("O"));
        command.Parameters.AddWithValue("$reduction", CalculatePercentSizeReduction(conversion.OriginalSize, conversion.ConvertedSize));
        command.Parameters.AddWithValue("$crf", conversion.Crf);
        command.Parameters.AddWithValue("$codec", conversion.VideoCodec);
        command.Parameters.AddWithValue("$duration", conversion.DurationSeconds);
        command.ExecuteNonQuery();
    }

    public LifetimeConversionStatsResponse GetStats()
    {
        using var connection = OpenConnection();
        using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT COUNT(*), COALESCE(SUM(OriginalSize), 0), COALESCE(SUM(ConvertedSize), 0),
                   COALESCE(AVG(Crf), 0), COALESCE(SUM(DurationSeconds), 0),
                   MIN(DateTimeConverted), MAX(DateTimeConverted), COALESCE(MAX(Id), 0)
            FROM ConversionHistory;
            """;
        using var reader = command.ExecuteReader();
        reader.Read();
        var originalSize = reader.GetInt64(1);
        var convertedSize = reader.GetInt64(2);
        return new LifetimeConversionStatsResponse(reader.GetInt64(0), originalSize, convertedSize,
            originalSize - convertedSize, CalculatePercentSizeReduction(originalSize, convertedSize),
            reader.GetDouble(3), reader.GetDouble(4),
            reader.IsDBNull(5) ? null : ParseTimestamp(reader.GetString(5)),
            reader.IsDBNull(6) ? null : ParseTimestamp(reader.GetString(6)), reader.GetInt64(7));
    }

    public ConversionHistoryPageResponse GetHistory(int page, int pageSize, long snapshotId)
    {
        if (page < 0 || pageSize < 1 || pageSize > 100 || snapshotId < 0)
            throw new ArgumentOutOfRangeException(nameof(page));

        using var connection = OpenConnection();
        using var countCommand = connection.CreateCommand();
        countCommand.CommandText = "SELECT COUNT(*) FROM ConversionHistory WHERE Id <= $snapshot;";
        countCommand.Parameters.AddWithValue("$snapshot", snapshotId);
        var totalCount = (long)countCommand.ExecuteScalar()!;
        using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT ConversionId, OriginalSize, ConvertedSize, InputFileName, OutputFileName,
                   InputPath, OutputPath, DateTimeConverted, PercentSizeReduction, Crf, VideoCodec, DurationSeconds
            FROM ConversionHistory WHERE Id <= $snapshot
            ORDER BY DateTimeConverted DESC, Id DESC LIMIT $limit OFFSET $offset;
            """;
        command.Parameters.AddWithValue("$snapshot", snapshotId);
        command.Parameters.AddWithValue("$limit", pageSize);
        command.Parameters.AddWithValue("$offset", (long)page * pageSize);
        using var reader = command.ExecuteReader();
        var items = new List<ConversionHistoryResponse>();
        while (reader.Read())
        {
            items.Add(new ConversionHistoryResponse(Guid.Parse(reader.GetString(0)), reader.GetInt64(1),
                reader.GetInt64(2), reader.GetString(3), reader.GetString(4), reader.GetString(5),
                reader.GetString(6), ParseTimestamp(reader.GetString(7)), reader.GetDouble(8),
                reader.GetInt32(9), reader.GetString(10), reader.GetDouble(11)));
        }
        return new ConversionHistoryPageResponse(items, page, pageSize, totalCount);
    }

    public static double CalculatePercentSizeReduction(long originalSize, long convertedSize)
        => originalSize == 0 ? 0 : (originalSize - (double)convertedSize) / originalSize * 100;

    private static DateTimeOffset ParseTimestamp(string value)
        => DateTimeOffset.Parse(value, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind);

    private SqliteConnection OpenConnection()
    {
        var connection = new SqliteConnection(connectionString);
        connection.Open();
        return connection;
    }
}