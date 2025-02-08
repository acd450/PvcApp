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
            .Where(f => f.Extension is ".mp4" or ".mkv")
            .ToArray();
        
        var statsList = new List<FileStats>();

        foreach (var videoFile in files)
        {
            var probe = FFProbe.Analyse(videoFile.FullName);
            var fileSizeMB = videoFile.Length >> 20;
            var isH264 = probe.PrimaryVideoStream?.CodecName.ToUpper() == "H264";
            var videoStats = new FileStats
            {
                FullPath = videoFile.FullName,
                IsH264File = isH264,
                SizeMB = fileSizeMB,
                PossibleMBSavings = fileSizeMB * (isH264 ? .3:.03) //Assume 30% savings for h264 and 3% otherwise
            };
            statsList.Add(videoStats);
            
            logger.Info(JsonSerializer.Serialize(videoStats));
        }
        
        var stats = new FolderStats
        {
            FullPath = WorkingDirectory,
            SizeGB = (statsList.Sum(s => s.SizeMB)/1024) + "GB",
            H264FileNames = statsList.Where(s => s.IsH264File).Select(s => s.FullPath).ToList(),
            PossibleSavings = statsList.Where(s => s.IsH264File).Sum(s => s.PossibleMBSavings/1024) + "GB",
        };
        
        return stats;
    }
}