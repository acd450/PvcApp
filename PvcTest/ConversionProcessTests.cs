using PlexVideoConverter.Models;

namespace PvcTest;

public class ConversionProcessTests
{
    private string tempDir = null!;

    [SetUp]
    public void Setup()
    {
        tempDir = Path.Combine(Path.GetTempPath(), "pvc-process-tests", Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);
    }

    [TearDown]
    public void Cleanup()
    {
        if (Directory.Exists(tempDir))
            Directory.Delete(tempDir, true);
    }

    [TestCase("movie.x264.mp4", "movie.x265.mkv")]
    [TestCase("movie.H264.avi", "movie.H265.mkv")]
    [TestCase("movie.h.264.mkv", "movie.h.265.mkv")]
    [TestCase("movie.1080p.mp4", "movie.1080p.mkv")]
    [TestCase(@"sub264\movie.x264.mp4", "movie.x265.mkv")]
    public void BuildOutputNameReplaces264With265InFileNameOnly(string inputName, string expectedOutputName)
    {
        Assert.That(ConversionProcess.BuildOutputName(inputName), Is.EqualTo(expectedOutputName));
    }

    [Test]
    public void FileSystemEventArgsConstructorReplaces264With265()
    {
        var args = new FileSystemEventArgs(WatcherChangeTypes.Created, tempDir, "Show.S01E01.x264.mp4");
        var process = new ConversionProcess(args);

        Assert.That(process.OutputName, Is.EqualTo("Show.S01E01.x265.mkv"));
    }

    [Test]
    public void FileStatsConstructorReplaces264With265()
    {
        var filePath = Path.Combine(tempDir, "Show.S01E01.H264.avi");
        File.WriteAllBytes(filePath, [1, 2, 3, 4]);

        var fileStats = new FileStats { FullPath = filePath, FileName = "Show.S01E01.H264.avi" };
        var process = new ConversionProcess(fileStats);

        Assert.That(process.OutputName, Is.EqualTo("Show.S01E01.H265.mkv"));
    }
}
