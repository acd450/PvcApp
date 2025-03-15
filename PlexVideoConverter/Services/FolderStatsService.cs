using System.Text.Json;
using FFMpegCore;
using NLog;
using PlexVideoConverter.Models;

namespace PlexVideoConverter.Services;

public class FolderStatsService
{
    
    private static Logger logger = LogManager.GetCurrentClassLogger();
        
    private static readonly Lazy<FolderStatsService> _instance = new (() => new FolderStatsService());
    public static FolderStatsService Instance => _instance.Value;

    public string WorkingDirectory { get; set; } = "";

    /// <summary>
    /// Gets statistics for all the supported video files contained within the working directory. This is not recursive.
    /// </summary>
    /// <returns></returns>
    /// <exception cref="NullReferenceException"></exception>
    public FolderStats GetWorkingDirectoryStats()
    {
        if (String.IsNullOrWhiteSpace(WorkingDirectory))
            throw new NullReferenceException("WorkingDirectory cannot be null or empty.");
        
        DirectoryInfo directory = new(WorkingDirectory);
        FileInfo[] files = directory.GetFiles("*.*", SearchOption.TopDirectoryOnly)
            .Where(f => f.Extension is ".mp4" or ".mkv" or ".avi")
            .ToArray();
        
        var statsList = new List<FileStats>();

        foreach (var videoFile in files)
        {
            var probe = FFProbe.Analyse(videoFile.FullName);
            var fileSizeGB = (double)(videoFile.Length >> 20)/1024;
            var isH264 = probe.PrimaryVideoStream?.CodecName.ToUpper() == "H264";
            var videoStats = new FileStats
            {
                FullPath = videoFile.FullName,
                IsH264File = isH264,
                SizeGB = fileSizeGB,
                PossibleGBSavings = fileSizeGB * (isH264 ? .3:.03) //Assume 30% savings for h264 and 3% otherwise
            };
            statsList.Add(videoStats);
            
            logger.Info(JsonSerializer.Serialize(videoStats));
        }

        var totalSize = statsList.Sum(s => s.SizeGB);
        var totalSavings = statsList.Where(s => s.IsH264File).Sum(s => s.PossibleGBSavings);
        
        var stats = new FolderStats
        {
            FullPath = WorkingDirectory,
            SizeGB = Math.Round(totalSize, 3) + "GB",
            H264FileNames = statsList.Where(s => s.IsH264File).Select(s => s.FullPath).ToList(),
            PossibleSavings = Math.Round(totalSavings, 3) + "GB",
        };
        
        return stats;
    }
}