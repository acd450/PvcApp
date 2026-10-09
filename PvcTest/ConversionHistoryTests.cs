using Microsoft.Data.Sqlite;
using PlexVideoConverter.Models;
using PlexVideoConverter.Services;

namespace PvcTest;

public class ConversionHistoryTests
{
    private string directory = null!;
    private string databasePath = null!;
    private ConversionHistoryService history = null!;

    [SetUp]
    public void Setup()
    {
        directory = Path.Combine(Path.GetTempPath(), "pvc-history-tests", Guid.NewGuid().ToString());
        databasePath = Path.Combine(directory, "conversions.sqlite");
        history = new ConversionHistoryService(databasePath);
    }

    [TearDown]
    public void Cleanup()
    {
        SqliteConnection.ClearAllPools();
        Directory.Delete(directory, true);
    }

    [Test]
    public void EmptyHistoryHasZeroTotals()
    {
        var stats = history.GetStats();
        Assert.That(stats.TotalConversions, Is.Zero);
        Assert.That(stats.PercentSizeReduction, Is.Zero);
        Assert.That(stats.FirstConversion, Is.Null);
        Assert.That(history.GetHistory(0, 25, stats.SnapshotId).Items, Is.Empty);
    }

    [Test]
    public void RecordsSurviveReopeningAndDuplicateCompletionIsIgnored()
    {
        var conversion = CreateConversion(1001, 501);
        history.Record(conversion);
        history.Record(conversion);
        var reopened = new ConversionHistoryService(databasePath);
        var stats = reopened.GetStats();
        var saved = reopened.GetHistory(0, 25, stats.SnapshotId).Items.Single();
        Assert.That(stats.TotalConversions, Is.EqualTo(1));
        Assert.That(saved, Is.EqualTo(conversion));
    }

    [Test]
    public void TotalsUseExactBytesAndWeightedReduction()
    {
        history.Record(CreateConversion(1000, 500));
        history.Record(CreateConversion(9000, 8100));
        var stats = history.GetStats();
        Assert.That(stats.OriginalSize, Is.EqualTo(10000));
        Assert.That(stats.ConvertedSize, Is.EqualTo(8600));
        Assert.That(stats.BytesSaved, Is.EqualTo(1400));
        Assert.That(stats.PercentSizeReduction, Is.EqualTo(14).Within(0.00001));
        Assert.That(stats.AverageCrf, Is.EqualTo(24));
        Assert.That(stats.TotalDurationSeconds, Is.EqualTo(10));
    }

    [Test]
    public void PagesAreNewestFirstAndDoNotShiftAfterNewConversions()
    {
        var first = CreateConversion(1000, 500, DateTimeOffset.UtcNow.AddMinutes(-2));
        var second = CreateConversion(2000, 1000, DateTimeOffset.UtcNow.AddMinutes(-1));
        history.Record(first);
        history.Record(second);
        var snapshot = history.GetStats().SnapshotId;
        history.Record(CreateConversion(3000, 1500));
        Assert.That(history.GetHistory(0, 1, snapshot).Items.Single(), Is.EqualTo(second));
        Assert.That(history.GetHistory(1, 1, snapshot).Items.Single(), Is.EqualTo(first));
        Assert.That(history.GetHistory(2, 1, snapshot).Items, Is.Empty);
        Assert.That(history.GetHistory(0, 1, snapshot).TotalCount, Is.EqualTo(2));
    }

    [TestCase(0, 0, 0)]
    [TestCase(100, 150, -50)]
    [TestCase(100, 25, 75)]
    public void ReductionHandlesEmptyAndLargerOutputs(long original, long converted, double expected)
    {
        history.Record(CreateConversion(original, converted));
        Assert.That(history.GetStats().PercentSizeReduction, Is.EqualTo(expected));
    }

    [TestCase(-1, 25, 0)]
    [TestCase(0, 0, 0)]
    [TestCase(0, 101, 0)]
    [TestCase(0, 25, -1)]
    public void InvalidPageRequestsAreRejected(int page, int pageSize, long snapshot)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => history.GetHistory(page, pageSize, snapshot));
    }

    private static ConversionHistoryResponse CreateConversion(long original, long converted, DateTimeOffset? completed = null)
        => new(Guid.NewGuid(), original, converted, "input's.avi", "output's.mkv",
            "C:/input/input's.avi", "C:/output/output's.mkv", completed ?? DateTimeOffset.UtcNow,
            ConversionHistoryService.CalculatePercentSizeReduction(original, converted), 24, "libx265", 5);
}