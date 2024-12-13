namespace PlexVideoConverter.Models;

/// <summary>
/// Tracks a File conversion process
/// </summary>
public class FileProcess
{
    public Guid Id { get; set; }
    public string FilePath { get; set; }
    public double Progress { get; set; }
    public string InputName { get; set; }
    public string OutputName { get; set; }

    public FileProcess(string filePath, double progress, string inputName, string outputName)
    {
        Id = Guid.NewGuid();
        FilePath = filePath;
        Progress = progress;
        InputName = inputName;
        OutputName = outputName;
    }

    public FileProcess(FileSystemEventArgs args)
    {
        Id = Guid.NewGuid();
        FilePath = args.FullPath;
        Progress = 0;
        InputName = args.Name ?? FilePath;
        OutputName = InputName.Substring(InputName.LastIndexOf("\\", StringComparison.Ordinal),
                InputName.Length - InputName.LastIndexOf("\\", StringComparison.Ordinal))
            .Replace(".mp4", ".mkv").Replace(".avi", ".mkv");
    }
}