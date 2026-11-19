namespace PlexVideoConverter;

/// <summary>
/// Path helpers that tolerate both Windows ('\') and Unix ('/') separators, since configured
/// folders and FileSystemWatcher event names can use either depending on the host OS.
/// </summary>
public static class PathUtils
{
    private static readonly char[] Separators = ['\\', '/'];

    /// <summary>
    /// Returns the final path segment. Unlike <see cref="Path.GetFileName(string)"/> this also
    /// splits on '\' when running on Unix.
    /// </summary>
    public static string GetFileName(string path)
    {
        if (string.IsNullOrEmpty(path)) return string.Empty;

        var index = path.LastIndexOfAny(Separators);
        return index < 0 ? path : path[(index + 1)..];
    }

    /// <summary>
    /// Rewrites separators in a configured path so it is usable on the current OS.
    /// </summary>
    public static string Normalize(string? path)
    {
        if (string.IsNullOrWhiteSpace(path)) return string.Empty;

        return OperatingSystem.IsWindows()
            ? path.Replace('/', '\\')
            : path.Replace('\\', '/');
    }
}
