using PlexVideoConverter.Models.FileBrowser;

namespace PlexVideoConverter.Models;

/// <summary>
/// Tracks a File conversion process
/// </summary>
public class ConversionProcess
{
    public Guid Id { get; set; }
    public string FilePath { get; set; }
    public double Progress { get; set; }
    public string InputName { get; set; }
    public string OutputName { get; set; }
    public string InputSizeGB { get; set; }
    public string OutputSizeGB { get; set; }

    public ConversionProcess(string filePath, double progress, string inputName, string outputName)
    {
        Id = Guid.NewGuid();
        FilePath = filePath;
        Progress = progress;
        InputName = inputName;
        OutputName = outputName;
    }

    public ConversionProcess(FileSystemEventArgs args)
    {
        Id = Guid.NewGuid();
        FilePath = args.FullPath;
        Progress = 0;
        InputName = args.Name ?? FilePath;
        OutputName = InputName.Substring(InputName.LastIndexOf("\\", StringComparison.Ordinal),
                InputName.Length - InputName.LastIndexOf("\\", StringComparison.Ordinal))
            .Replace(".mp4", ".mkv").Replace(".avi", ".mkv");
    }

    public ConversionProcess(FileStats file)
    {
        Id = Guid.NewGuid();
        FilePath = file.FullPath;
        Progress = 0;
        InputName = file.FileName;
        OutputName = InputName.Substring(InputName.LastIndexOf("\\", StringComparison.Ordinal),
                InputName.Length - InputName.LastIndexOf("\\", StringComparison.Ordinal))
            .Replace(".mp4", ".mkv").Replace(".avi", ".mkv");
        InputSizeGB = ((double)(File.Open(file.FullPath + file.FileName, FileMode.Open).Length >> 20)/1024).ToString();
    }
}