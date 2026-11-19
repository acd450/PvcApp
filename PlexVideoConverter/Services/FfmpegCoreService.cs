using FFMpegCore;
using FFMpegCore.Enums;
using NLog;
using PlexVideoConverter.Hubs;
using PlexVideoConverter.Models;

namespace PlexVideoConverter.Services;

public class FfmpegCoreService
{
    private static readonly Lazy<FfmpegCoreService> _instance = new (() => new FfmpegCoreService());
    public static FfmpegCoreService Instance => _instance.Value;
    
    private static Logger logger = LogManager.GetCurrentClassLogger();

    private SemaphoreSlim sem;
    private ConversionHistoryService? historyService;
    
    public Dictionary<Guid, ConversionProcess> FileProcesses { get; set; } = new();
    public Dictionary<Guid, ConversionProcess> CompletedFileProcesses { get; set; } = new();

    public FfmpegCoreService()
    {
        sem = new SemaphoreSlim(1, 1);
    }

    public static void InitializeHistory(ConversionHistoryService history)
    {
        Instance.historyService = history;
    }

    /// <summary>
    /// Test function to see if FFMPEG was working
    /// </summary>
    public static void TestConvertVideo()
    {
        var testFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
            "Videos", "ffmpeg-ToConvert");
        var inputName = Path.Combine(testFolder, "Keystone Instagram.mp4");
        var outputName = Path.Combine(testFolder, "Keystone Instagram-sm.mkv");
        
        try
        {
            FFMpegArguments.FromFileInput(inputName)
                .OutputToFile(outputName, false, options => options
                    .WithVideoCodec(VideoCodec.LibX265)
                    .WithConstantRateFactor(24)
                    .WithFastStart())
                .ProcessSynchronously();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    }

    public async Task ConvertVideoAsync(ConversionProcess fp)
    {
        try
        {
            var history = historyService ?? throw new InvalidOperationException("Conversion history is not initialized.");
            var outputPath =
                SettingsService.Instance.GetExportSettings().FirstOrDefault()?
                    .FolderPath;
            var outputFilePath = Path.Combine(outputPath ?? string.Empty, fp.OutputName.TrimStart('\\', '/'));


            // ffmpeg will not create a missing output directory itself
            if (!string.IsNullOrEmpty(outputPath)) Directory.CreateDirectory(outputPath);

            logger.Info($"Converting File: {fp.FilePath}");
            logger.Info($"Output File: {outputFilePath}");

            var percentTrackerFrontend = 0;
            var percentTrackerLogging = 0;
            var fpId = fp.Id;

            // Track this process so ProgressHandler can look it up and clients can see it as active
            FileProcesses[fpId] = fp;

            var videoDuration = FFProbe.Analyse(fp.FilePath).Duration;
            var videoQuality = SettingsService.Instance.FfmpegSettings?.videoQuality ?? 24;
            var originalSize = new FileInfo(fp.FilePath).Length;
            var reportPercentProgressFrontend = SettingsService.Instance.FfmpegSettings?.reportPercentProgressFrontend ?? 1;
            var reportPercentProgressLogging = SettingsService.Instance.FfmpegSettings?.reportPercentProgressLogging ?? 20;

            logger.Info($"Ffmpeg has crf={videoQuality}");

            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            var succeeded = await FFMpegArguments
                .FromFileInput(fp.FilePath)
                .OutputToFile(outputFilePath, false, options => options
                    .WithVideoCodec(VideoCodec.LibX265)
                    .WithConstantRateFactor(videoQuality)
                    .WithFastStart())
                .NotifyOnProgress(ProgressHandler, videoDuration)
                .ProcessAsynchronously();
            stopwatch.Stop();
            if (!succeeded)
                throw new InvalidOperationException("FFmpeg did not complete successfully.");

            var outputFile = new FileInfo(outputFilePath);
            var convertedSize = outputFile.Length;
            var reduction = ConversionHistoryService.CalculatePercentSizeReduction(originalSize, convertedSize);
            history.Record(new ConversionHistoryResponse(fp.Id, originalSize, convertedSize,
                PathUtils.GetFileName(fp.FilePath), outputFile.Name, Path.GetFullPath(fp.FilePath),
                outputFile.FullName, DateTimeOffset.UtcNow, reduction, videoQuality, "libx265", stopwatch.Elapsed.TotalSeconds));
            fp.InputSizeGB = (originalSize / Math.Pow(1024, 3)).ToString(System.Globalization.CultureInfo.InvariantCulture);
            fp.OutputSizeGB = (convertedSize / Math.Pow(1024, 3)).ToString(System.Globalization.CultureInfo.InvariantCulture);
            logger.Info($"Stats for {outputFile.Name}: {originalSize} -> {convertedSize} bytes, {reduction:F2}% saved");

            void ProgressHandler(double p)
            {
                //Update current progress
                FileProcesses[fpId].Progress = p;

                //Only log when the percent exceeds the logging report interval
                if (percentTrackerLogging < p / reportPercentProgressLogging)
                {
                    logger.Info("Current Video Progress: " + p + "%");
                    percentTrackerLogging = (int)Math.Ceiling(p / reportPercentProgressLogging);
                }

                //Only notify the frontend when the percent exceeds the frontend report interval
                if (percentTrackerFrontend < p / reportPercentProgressFrontend)
                {
                    PvcConversionClient.Instance.SendConversionProgressUpdate(fpId, (int)p);
                    percentTrackerFrontend = (int)Math.Ceiling(p / reportPercentProgressFrontend);
                }
            }
        }
        catch (Exception ex)
        {
            logger.Error("Error during video conversion. " + ex.Message, ex);
            FileProcesses.Remove(fp.Id);
            throw;
        }
    }

    /// <summary>
    /// Anything to run after the video conversion is complete. Currently moves files to a 
    /// </summary>
    /// <param name="fullPathFile"></param>
    public void CompleteFileConversion(ConversionProcess fp)
    {
        try
        {
            var fileName = PathUtils.GetFileName(fp.FilePath);

            var completedPath =
                SettingsService.Instance.GetPostImportSettings()?
                    .FolderPath;
            var destinationPath = Path.Combine(completedPath ?? string.Empty, fileName);

            // File.Move will not create a missing destination directory itself
            if (!string.IsNullOrEmpty(completedPath)) Directory.CreateDirectory(completedPath);

            logger.Info($"Finished converting video, moving to: {destinationPath}");

            File.Move(fp.FilePath, destinationPath);
            FileProcesses.Remove(fp.Id);
            CompletedFileProcesses.Add(fp.Id, fp);
        }
        catch (Exception ex)
        {
            logger.Error("Error during file conversion. " + ex.Message, ex);
        }
    }

}