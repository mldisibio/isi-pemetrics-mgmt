namespace PEMetrics.DataCache.Configuration;

/// <summary>Resolves cache file paths using MyDocuments prefix, absolute, or relative conventions.</summary>
public sealed class CachePathResolver
{
    const string MyDocumentsPrefix = "MyDocuments";
    readonly string _applicationBasePath;

    public CachePathResolver() : this(AppDomain.CurrentDomain.BaseDirectory) { }

    public CachePathResolver(string applicationBasePath)
    {
        _applicationBasePath = applicationBasePath ?? throw new ArgumentNullException(nameof(applicationBasePath));
    }

    /// <summary>Resolves a path according to the prefix conventions.</summary>
    /// <param name="path">Path with optional MyDocuments prefix, absolute path, or relative path.</param>
    /// <returns>Fully resolved absolute path.</returns>
    /// <remarks>
    /// Resolution rules:
    /// - Paths starting with "MyDocuments" → resolved to Environment.SpecialFolder.Personal
    /// - Absolute paths (rooted) → used as-is
    /// - Relative paths → resolved relative to application base directory
    /// </remarks>
    public string ResolvePath(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
            throw new ArgumentException("Path cannot be null or empty.", nameof(path));

        // Normalize path separators
        var normalizedPath = path.Replace('/', Path.DirectorySeparatorChar);

        // Check for MyDocuments prefix
        if (normalizedPath.StartsWith(MyDocumentsPrefix, StringComparison.OrdinalIgnoreCase))
        {
            var documentsFolder = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
            var relativePart = normalizedPath.Substring(MyDocumentsPrefix.Length).TrimStart(Path.DirectorySeparatorChar);
            return Path.Combine(documentsFolder, relativePart);
        }

        // Check if path is already absolute
        if (Path.IsPathRooted(normalizedPath))
            return normalizedPath;

        // Treat as relative to application base directory
        return Path.Combine(_applicationBasePath, normalizedPath);
    }

    /// <summary>Resolves the path and ensures the parent directory exists.</summary>
    /// <param name="path">Path to resolve.</param>
    /// <returns>Fully resolved absolute path with parent directory created.</returns>
    public string ResolvePathAndEnsureDirectory(string path)
    {
        var resolvedPath = ResolvePath(path);
        var directory = Path.GetDirectoryName(resolvedPath);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }
        return resolvedPath;
    }
}
