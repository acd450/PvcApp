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
    
    public Dictionary<Guid, ConversionProcess> FileProcesses { get; set; } = new();
    public Dictionary<Guid, ConversionProcess> CompletedFileProcesses { get; set; } = new();

    public FfmpegCoreService()
    {
        sem = new SemaphoreSlim(1, 1);
    }

    /// <summary>
    /// Test function to see if FFMPEG was working
    /// </summary>
    public static void TestConvertVideo()
    {
        var inputName = "C:\\Users\\user\\Videos\\ffmpeg-ToConvert\\Keystone Instagram.mp4";
        var outputName = "C:\\Users\\user\\Videos\\ffmpeg-ToConvert\\Keystone Instagram-sm.mkv";
        
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

    public Task ConvertVideoAsync(ref ConversionProcess fp)
    {
        try
        {
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
            var reportPercentProgressFrontend = SettingsService.Instance.FfmpegSettings?.reportPercentProgressFrontend ?? 1;
            var reportPercentProgressLogging = SettingsService.Instance.FfmpegSettings?.reportPercentProgressLogging ?? 20;

            logger.Info($"Ffmpeg has crf={videoQuality}");

            return FFMpegArguments
                .FromFileInput(fp.FilePath)
                .OutputToFile(outputFilePath, false, options => options
                    .WithVideoCodec(VideoCodec.LibX265)
                    .WithConstantRateFactor(videoQuality)
                    .WithFastStart())
                .NotifyOnProgress(ProgressHandler, videoDuration)
                .ProcessAsynchronously();

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
            return Task.CompletedTask;
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
            var fileName = fp.FilePath.Substring(fp.FilePath.LastIndexOf("\\", StringComparison.Ordinal),
                fp.FilePath.Length - fp.FilePath.LastIndexOf("\\", StringComparison.Ordinal));

            var completedPath =
                SettingsService.Instance.GetPostImportSettings()?
                    .FolderPath;
            var destinationPath = Path.Combine(completedPath ?? string.Empty, fileName.TrimStart('\\', '/'));

            // File.Move will not create a missing destination directory itself
            if (!string.IsNullOrEmpty(completedPath)) Directory.CreateDirectory(completedPath);

            CalculateConversionStats(fp.FilePath);

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

    private void CalculateConversionStats(string inputFilePath)
    {
        var outputPath =
            SettingsService.Instance.GetExportSettings().FirstOrDefault()?
                .FolderPath;

        var outputFileName = inputFilePath.Substring(inputFilePath.LastIndexOf("\\", StringComparison.Ordinal),
                inputFilePath.Length - inputFilePath.LastIndexOf("\\", StringComparison.Ordinal))
            .Replace(".mp4", ".mkv");
        var outputFilePath = Path.Combine(outputPath ?? string.Empty, outputFileName.TrimStart('\\', '/'));

        var fiInput = new FileInfo(inputFilePath);
        var fiOutput = new FileInfo(outputFilePath);

        if (!fiInput.Exists)
        {
            logger.Error("Cannot find Input file for final stats. File Path: " + inputFilePath);
            return;
        } if (!fiOutput.Exists)
        {
            logger.Error("Cannot find Output file for final stats. File Path: " + outputFilePath);
            return;
        }

        var inputFileSize = fiInput.Length >> 20;
        var outputFileSize = fiOutput.Length >> 20;
        var savingsPercent = ((double)inputFileSize - outputFileSize) / inputFileSize * 100;
        logger.Info("Stats for file: " + fiOutput.Name);
        logger.Info("Input File Size: " + inputFileSize + " MB");
        logger.Info("Output File Save: " + outputFileSize + " MB");
        logger.Info("Total conversion savings: " + Math.Floor(savingsPercent) + "%");
    }
}