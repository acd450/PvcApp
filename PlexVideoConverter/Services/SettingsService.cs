using System.Text.Json;
using NLog;
using PlexVideoConverter.Models;

namespace PlexVideoConverter.Services;

public class SettingsService
{
    private static Logger logger = LogManager.GetCurrentClassLogger();
    
    private static SettingsService _instance;
    
    public static SettingsService Instance => _instance == null ? new SettingsService() : _instance;

    public List<FileListenerSettings> FileListenerSettings = new();
    
    public FfmpegSettings? FfmpegSettings = new();
    
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