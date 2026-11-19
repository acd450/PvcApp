namespace PlexVideoConverter.Models;

/// <summary>A successfully converted file, with exact sizes in bytes and UTC timestamps.</summary>
public sealed record ConversionHistoryResponse(
    Guid ConversionId,
    long OriginalSize,
    long ConvertedSize,
    string InputFileName,
    string OutputFileName,
    string InputPath,
    string OutputPath,
    DateTimeOffset DateTimeConverted,
    double PercentSizeReduction,
    int Crf,
    string VideoCodec,
    double DurationSeconds);

/// <summary>Lifetime totals and a stable history snapshot for paged browsing.</summary>
public sealed record LifetimeConversionStatsResponse(
    long TotalConversions,
    long OriginalSize,
    long ConvertedSize,
    long BytesSaved,
    double PercentSizeReduction,
    double AverageCrf,
    double TotalDurationSeconds,
    DateTimeOffset? FirstConversion,
    DateTimeOffset? LatestConversion,
    long SnapshotId);

/// <summary>A newest-first page of completed conversions.</summary>
public sealed record ConversionHistoryPageResponse(
    IReadOnlyList<ConversionHistoryResponse> Items,
    int Page,
    int PageSize,
    long TotalCount);