namespace PlexVideoConverter.Models.FileBrowser;

public class FileChildrenRequest
{
    public string Path { get; set; }
    public bool IncludeFiles { get; set; } = true;
    public bool IncludeDirectories { get; set; } = true;
}