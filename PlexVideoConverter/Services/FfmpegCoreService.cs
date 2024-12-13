using FFMpegCore;
using FFMpegCore.Enums;
using NLog;
using PlexVideoConverter.Models;

namespace PlexVideoConverter.Services;

public class FfmpegCoreService
{
    private static FfmpegCoreService _instance;
    public static FfmpegCoreService Instance => _instance ??= new FfmpegCoreService();
    
    private static Logger logger = LogManager.GetCurrentClassLogger();

    private SemaphoreSlim sem;
    
    public Dictionary<Guid, FileProcess> FileProcesses { get; set; } = new();
    public Dictionary<Guid, FileProcess> CompletedFileProcesses { get; set; } = new();

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

    public async void AddItems(FileProcess fp)
    {
        FileProcesses.Add(fp.Id, fp);
        await Instance.Enqueue(() => Instance.ConvertVideoAsync(ref fp));
        
        Instance.CompleteFileConversion(fp);
    }
    
    public async Task Enqueue(Func<Task> taskGenerator)
    {
        await sem.WaitAsync();
        try
        {
            logger.Info("Tasked started...");
            await taskGenerator();
        }
        finally
        {
            logger.Info("Task finished processing...");
            sem.Release();
        }
    }

    private Task ConvertVideoAsync(ref FileProcess fp)
    {
        try
        {
            var outputPath =
                SettingsService.Instance.GetExportSettings().FirstOrDefault()?
                    .FolderPath;

            logger.Info($"Converting File: {fp.FilePath}");
            logger.Info($"Output File: {outputPath + fp.OutputName}");

            var percentTracker = 0;
            var fpId = fp.Id;

            var videoDuration = FFProbe.Analyse(fp.FilePath).Duration;
            var videoQuality = SettingsService.Instance.FfmpegSettings?.videoQuality ?? 24;
            var reportPercentProgress = SettingsService.Instance.FfmpegSettings?.reportPercentProgress ?? 10;

            logger.Info($"Ffmpeg has crf={videoQuality}");

            return FFMpegArguments
                .FromFileInput(fp.FilePath)
                .OutputToFile(outputPath + fp.OutputName, false, options => options
                    .WithVideoCodec(VideoCodec.LibX265)
                    .WithConstantRateFactor(videoQuality)
                    .WithFastStart())
                .NotifyOnProgress(ProgressHandler, videoDuration)
                .ProcessAsynchronously();

            void ProgressHandler(double p)
            {
                //Update current progress
                FileProcesses[fpId].Progress = p;
                //Only log when the percent exceeds the reportPercentCompletion
                if (percentTracker < p / reportPercentProgress)
                {
                    logger.Info("Current Video Progress: " + p + "%");
                    percentTracker = (int)Math.Ceiling(p / reportPercentProgress);
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
    public void CompleteFileConversion(FileProcess fp)
    {
        var fileName = fp.FilePath.Substring(fp.FilePath.LastIndexOf("\\", StringComparison.Ordinal),
            fp.FilePath.Length - fp.FilePath.LastIndexOf("\\", StringComparison.Ordinal));
        
        var completedPath =
            SettingsService.Instance.GetPostImportSettings()?
                .FolderPath;
        
        CalculateConversionStats(fp.FilePath);
        
        logger.Info($"Finished converting video, moving to: {completedPath + fileName}");
        
        File.Move(fp.FilePath, completedPath + fileName);
        FileProcesses.Remove(fp.Id); CompletedFileProcesses.Add(fp.Id, fp);
    }

    private void CalculateConversionStats(string inputFilePath)
    {
        var outputPath =
            SettingsService.Instance.GetExportSettings().FirstOrDefault()?
                .FolderPath;

        var outputFileName = inputFilePath.Substring(inputFilePath.LastIndexOf("\\", StringComparison.Ordinal),
                inputFilePath.Length - inputFilePath.LastIndexOf("\\", StringComparison.Ordinal))
            .Replace(".mp4", ".mkv");

        var fiInput = new FileInfo(inputFilePath);
        var fiOutput = new FileInfo(outputPath + outputFileName);

        if (!fiInput.Exists)
        {
            logger.Error("Cannot find Input file for final stats. File Path: " + inputFilePath);
            return;
        } if (!fiOutput.Exists)
        {
            logger.Error("Cannot find Output file for final stats. File Path: " + outputPath + outputFileName);
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