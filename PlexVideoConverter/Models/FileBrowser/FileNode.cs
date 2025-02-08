namespace PlexVideoConverter.Models.FileBrowser;

public class FileNode
{
    public string Name { get; set; }
    public string Path { get; set;  }
    public bool IsDirectory { get; set; }
    public bool HasChildren { get; set; }
}