using System.Diagnostics;
using System.Text.Json;
using NLog;
using PlexVideoConverter.Models;

namespace PlexVideoConverter.Services;

public class SettingsService
{
    private static Logger logger = LogManager.GetCurrentClassLogger();

    private const string SettingsFileName = "fileListenerSettings.json";
    
    private static readonly Lazy<SettingsService> _instance = new (() => new SettingsService());
    public static SettingsService Instance => _instance.Value;

    public List<FileListenerSettings> FileListenerSettings = new();

    public FfmpegSettings? FfmpegSettings { get; set; } = new();

    public Process? npmProcess { get; set; }

    /// <summary>
    /// Directory holding fileListenerSettings.json. Falls back to a per-user application data
    /// folder when the configured value is missing or belongs to another OS.
    /// </summary>
    public string SettingsLocation => ResolveSettingsLocation(FfmpegSettings?.ffmpegSettingsLocation);

    private static string ResolveSettingsLocation(string? configured)
    {
        if (!string.IsNullOrWhiteSpace(configured) && IsUsableOnThisPlatform(configured))
            return PathUtils.Normalize(configured);

        if (File.Exists(Path.Combine(AppContext.BaseDirectory, SettingsFileName)))
            return AppContext.BaseDirectory;

        return Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "PlexVideoConverter");
    }

    /// <summary>
    /// A drive-letter path is meaningless off Windows and a rooted Unix path is meaningless on Windows.
    /// </summary>
    private static bool IsUsableOnThisPlatform(string path)
    {
        var isWindowsStyle = path.Length > 1 && path[1] == ':';
        return isWindowsStyle == OperatingSystem.IsWindows();
    }

    public void PopulateGlobalSettings()
    {
        try
        {
            var settingsLocation = SettingsLocation;

            //Check if program data directory exists, if not make it.
            if (!Directory.Exists(settingsLocation))
            {
                logger.Info($"Creating directory for fileListenerSettings.json in Location: {settingsLocation}");
                logger.Info("You will need to create fileListenerSettings.json file and place it there.");
                Directory.CreateDirectory(settingsLocation);
            }
            
            using (StreamReader r = new StreamReader(Path.Combine(settingsLocation, SettingsFileName)))
            {
                string json = r.ReadToEnd();
                var fileListenerSettings = JsonSerializer.Deserialize<List<FileListenerSettings>>(json);
                if (fileListenerSettings != null)
                    FileListenerSettings = fileListenerSettings;

                FileListenerSettings.ForEach(s => s.FolderPath = PathUtils.Normalize(s.FolderPath));
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error in PopulateGlobalSettings. " + ex.Message);
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