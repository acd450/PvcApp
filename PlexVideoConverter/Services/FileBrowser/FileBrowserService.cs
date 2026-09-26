using NLog;
using PlexVideoConverter.Models.FileBrowser;

namespace PlexVideoConverter.Services.FileBrowser;

public class FileBrowserService
{
    private static Logger logger = LogManager.GetCurrentClassLogger();
    
    /// <summary>
    /// Gets the list of drives connected to the server/computer
    /// </summary>
    /// <returns></returns>
    public static List<DriveNode> GetRootDrives()
    {
        
        var driveNodes = new List<DriveNode>();

        // Full list will be both Mapped and NonMapped Drives, so add all the Mapped drives 
        driveNodes.AddRange(GetDriveNodeList());
        
        // Ensure no repeated drives and sort by drive letter
        return driveNodes.Distinct()
            .OrderBy(o => o.driveLetter)
            .ToList();
    }

    /// <summary>
    /// Gets the children of a root or Folder to compile a list of its contents
    /// </summary>
    /// <param name="req">FileChildrenRequest containing root directory and whether to get files/folders</param>
    /// <returns></returns>
    public static List<FileNode> GetDirectoryChildrenNodes(FileChildrenRequest req)
    {
        // A tuple where the second entry is whether or not the path is a directory
        List<Tuple<string, bool>> childPathsIsDir = [];
        
        // If the request path is empty then get the Root Drives
        if (string.IsNullOrEmpty(req.Path))
        {
            if (!req.IncludeDirectories) return new List<FileNode>();
            childPathsIsDir = GetRootDrives().Select(drive => new Tuple<string, bool>(drive.drivePath, true)).ToList();
        }
        // Otherwise massage the provided path and get its children
        else
        {
            var correctedPath = NormalizeRequestPath(req.Path);

            if (req.IncludeDirectories)
                childPathsIsDir.AddRange(Directory.GetDirectories(correctedPath).Select(dirPath => new Tuple<string, bool>(dirPath, true)));
            if (req.IncludeFiles)
                childPathsIsDir.AddRange(Directory.GetFiles(correctedPath).Select(filePath => new Tuple<string, bool>(filePath, false)));
        }
        
        // Sort Directories alphabetically first then files 
        childPathsIsDir.Sort((a, b) =>
        {
            var result = b.Item2.CompareTo(a.Item2);
            return result == 0 ? string.Compare(a.Item1, b.Item1, StringComparison.Ordinal) : result;
        });
        
        var children = new List<FileNode>();
        childPathsIsDir.ForEach(child =>
        {
            try
            {
                children.Add(GetListChildrenFromTuple(child, req.IncludeFiles));
            }
            catch (UnauthorizedAccessException)
            {
                logger.Warn($"Access denied for location: {child.Item1}");
            }
            catch (IOException)
            {
                logger.Warn($"Device not ready: {child.Item1}");
            }
        });
        
        return children;
    }

    /// <summary>
    /// Populates file nodes based on path and whether its a directory or file
    /// </summary>
    /// <param name="pathIsDir">Tuple containing path as Item1 and is Directory as Item2</param>
    /// <param name="includeFiles">Whether to include files</param>
    private static FileNode GetListChildrenFromTuple(Tuple<string, bool> pathIsDir, bool includeFiles = true)
    {
        var path = pathIsDir.Item1;
        var isDirectory = pathIsDir.Item2;
        var childCount = !isDirectory ? 0 
            : includeFiles ? Directory.GetFileSystemEntries(path).Length
            : Directory.GetDirectories(path).Length;
            
        var name = Path.GetFileName(path);
        return new FileNode
        {
            Name = string.IsNullOrEmpty(name) ? path : name,
            Path = path,
            IsDirectory = isDirectory,
            HasChildren = childCount > 0
        };
    }
    
    /// <summary>
    /// Converts a path coming from the browser into one the local file system understands.
    /// </summary>
    private static string NormalizeRequestPath(string path)
    {
        var normalized = PathUtils.Normalize(path);

        // Windows rejects a bare "C:" so keep the trailing separator on drive roots
        if (OperatingSystem.IsWindows() && normalized.Length == 2 && normalized[1] == ':')
            normalized += Path.DirectorySeparatorChar;

        return normalized;
    }

    /// <summary>
    /// Gets a list of all the local mapped drives
    /// </summary>
    /// <returns>List of type DriveNode</returns>
    private static List<DriveNode> GetDriveNodeList()
    {
        var driveNodes = new List<DriveNode>();

        try
        {
            foreach (var rootName in GetDriveListAsStrings())
                driveNodes.Add(new DriveNode { driveLetter = rootName, drivePath = rootName });

            return driveNodes;
        }
        catch(Exception ex)
        {
            logger.Error($"Exception during FileBrowserService.GetMappedDriveList: {ex.Message}", ex);
            return driveNodes;
        }
    }
    
    /// <summary>
    /// Returns the browsable roots: drive letters on Windows, "/" plus mounted volumes elsewhere.
    /// </summary>
    /// <returns>List of string names for the drives</returns>
    public static List<string> GetDriveListAsStrings()
    {
        try
        {
            if (OperatingSystem.IsWindows())
                return DriveInfo.GetDrives().Select(d => d.Name).ToList();

            // DriveInfo on macOS/Linux also reports dozens of pseudo file systems, so list the
            // filesystem root and any mounted volumes instead.
            var roots = new List<string> { "/" };
            foreach (var mountRoot in new[] { "/Volumes", "/media", "/mnt" })
            {
                if (Directory.Exists(mountRoot))
                    roots.AddRange(Directory.GetDirectories(mountRoot));
            }

            return roots;
        }
        catch(Exception ex)
        {
            logger.Error($"Exception during FileBrowserService.GetMappedDriveListAsStrings: {ex.Message}", ex);
            return [];
        }
    }
}