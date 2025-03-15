namespace PlexVideoConverter.Models;

public class FolderStats
{
    public string FullPath { get; set; }
    public string SizeGB { get; set; }
    public List<string> H264FileNames { get; set; } = new List<string>();
    public string PossibleSavings { get; set; }
}

public class FileStats
{
    public string FullPath { get; set; }
    public double SizeGB { get; set; }
    public bool IsH264File { get; set; }
    public double PossibleGBSavings { get; set; }
}