namespace PlexVideoConverter.Models.FileBrowser;

public class DriveNode: IEquatable<DriveNode>
{
    public string driveLetter { get; set; }
    public string drivePath { get; set; }
    
    
    public bool Equals(DriveNode other)
    {
        return driveLetter.Equals(other.driveLetter) && drivePath.Equals(other.drivePath);
    }
    
    public override int GetHashCode()
    {
        return HashCode.Combine(driveLetter, drivePath);
    }
}