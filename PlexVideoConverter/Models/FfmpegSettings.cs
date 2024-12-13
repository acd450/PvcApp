namespace PlexVideoConverter.Models;

public class FfmpegSettings
{
    public int videoQuality { get; set; }
    public int reportPercentProgress { get; set; }
    public string ffmpegSettingsLocation { get; set; }
}