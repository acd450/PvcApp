namespace PlexVideoConverter.Models;

public class FfmpegSettings
{
    public int videoQuality { get; set; }
    public int reportPercentProgressFrontend { get; set; } = 1;
    public int reportPercentProgressLogging { get; set; } = 20;
    public string ffmpegSettingsLocation { get; set; }
}