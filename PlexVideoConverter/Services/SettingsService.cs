using System.Diagnostics;
using System.Text.Json;
using NLog;
using PlexVideoConverter.Models;

namespace PlexVideoConverter.Services;

public class SettingsService
{
    private static Logger logger = LogManager.GetCurrentClassLogger();
    
    private static readonly Lazy<SettingsService> _instance = new (() => new SettingsService());
    public static SettingsService Instance => _instance.Value;

    public List<FileListenerSettings> FileListenerSettings = new();

    public FfmpegSettings? FfmpegSettings { get; set; } = new();

    public Process? npmProcess { get; set; }
    
    public void PopulateGlobalSettings()
    {
        try
        {
            //Check if program data directory exists, if not make it.
            if (!Directory.Exists(FfmpegSettings.ffmpegSettingsLocation))
            {
                logger.Info($"Creating directory for fileListenerSettings.json in Location: {FfmpegSettings.ffmpegSettingsLocation}");
                logger.Info("You will need to create fileListenerSettings.json file and place it there.");
                Directory.CreateDirectory(FfmpegSettings.ffmpegSettingsLocation);
            }
            
            using (StreamReader r = new StreamReader(FfmpegSettings.ffmpegSettingsLocation + "\\fileListenerSettings.json"))
            {
                string json = r.ReadToEnd();
                var fileListenerSettings = JsonSerializer.Deserialize<List<FileListenerSettings>>(json);
                if (fileListenerSettings != null)
                    FileListenerSettings = fileListenerSettings;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error in GlobalSettingsService. " + ex.Message);
            logger.Error("Error in PopulateGlobalSettings. " + ex.Message, ex);
        }
    }
    
    public List<FileListenerSettings> GetImportSettings()
    {
        return Instance.FileListenerSettings.FindAll(setting => setting.FolderType == "IMPORT");
    }

    public FileListenerSettings? GetPostImportSettings()
    {
        return Instance.FileListenerSettings.FirstOrDefault(setting => setting.FolderType == "POST-IMPORT");
    }

    public List<FileListenerSettings> GetExportSettings()
    {
        return Instance.FileListenerSettings.FindAll(setting => setting.FolderType == "EXPORT");
    }
}