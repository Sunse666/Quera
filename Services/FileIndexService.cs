namespace Quera.Services;

public class FileIndexService : IFileIndexService
{
    private readonly ILogger<FileIndexService> _logger;
    private readonly IConfigService _configService;
    private List<SearchResult> _cache = new();

    public bool CacheBuilt { get; private set; }
    public List<SearchResult> Cache => _cache;

    public FileIndexService(ILogger<FileIndexService> logger, IConfigService configService)
    {
        _logger = logger;
        _configService = configService;
    }

    public async Task BuildCacheAsync(List<string> searchPaths, List<string> fileTypes)
    {
        if (!_configService.Current.Cache.Enabled) return;

        var cache = new List<SearchResult>();

        var typeSet = new HashSet<string>(
            fileTypes.Select(t => t.StartsWith(".") ? t.ToLowerInvariant() : "." + t.ToLowerInvariant()));

        var excludePaths = _configService.Current.Exclude.Paths
            .Select(p => _configService.ExpandPath(p).ToLowerInvariant()).ToHashSet();
        var excludePatterns = _configService.Current.Exclude.Patterns
            .Select(p => p.ToLowerInvariant()).ToHashSet();
        var maxDepth = _configService.Current.Search.MaxDepth;
        var maxFiles = _configService.Current.Cache.MaxFiles;

        var customFiles = await Task.Run(() =>
        {
            var results = new List<SearchResult>();
            foreach (var rawPath in searchPaths)
            {
                if (results.Count >= maxFiles) break;
                var expanded = _configService.ExpandPath(rawPath);
                if (!Directory.Exists(expanded)) continue;
                ScanDirectory(expanded, typeSet, results, excludePaths, excludePatterns, maxDepth, 0, maxFiles);
            }
            return results;
        });

        var startMenuFiles = await Task.Run(() =>
        {
            var results = new List<SearchResult>();
            var startMenuPaths = new[]
            {
                Environment.GetFolderPath(Environment.SpecialFolder.StartMenu),
                Environment.GetFolderPath(Environment.SpecialFolder.CommonStartMenu)
            };

            foreach (var menuPath in startMenuPaths)
            {
                if (!Directory.Exists(menuPath)) continue;
                try
                {
                    foreach (var file in Directory.EnumerateFiles(menuPath, "*.lnk", SearchOption.AllDirectories))
                    {
                        results.Add(new SearchResult
                        {
                            Name = Path.GetFileNameWithoutExtension(file),
                            Description = file,
                            Path = file,
                            Extension = ".lnk",
                            Type = SearchResultType.App,
                            Icon = "\U0001F517",
                            Source = "startmenu",
                            SortOrder = 3,
                            ExtOrder = 2
                        });
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to index start menu: {Path}", menuPath);
                }
            }
            return results;
        });

        cache.AddRange(customFiles);
        cache.AddRange(startMenuFiles);

        _cache = cache;
        CacheBuilt = true;
    }

    private static void ScanDirectory(string dir, HashSet<string> typeSet, List<SearchResult> results,
        HashSet<string> excludePaths, HashSet<string> excludePatterns, int maxDepth, int depth, int maxFiles)
    {
        if (results.Count >= maxFiles) return;
        if (maxDepth >= 0 && depth > maxDepth) return;
        if (excludePaths.Contains(dir.ToLowerInvariant())) return;

        try
        {
            foreach (var file in Directory.EnumerateFiles(dir, "*.*"))
            {
                if (results.Count >= maxFiles) break;
                if (excludePatterns.Any(p => MatchPattern(Path.GetFileName(file), p))) continue;

                var ext = Path.GetExtension(file).ToLowerInvariant();
                if (!ext.StartsWith(".")) ext = "." + ext;
                if (!typeSet.Contains(ext)) continue;

                results.Add(new SearchResult
                {
                    Name = Path.GetFileNameWithoutExtension(file),
                    Description = file,
                    Path = file,
                    Extension = ext,
                    Type = SearchResultType.File,
                    Icon = GetFileIcon(ext),
                    Source = "custom",
                    SortOrder = 1,
                    ExtOrder = GetExtOrder(ext)
                });
            }

            foreach (var subDir in Directory.EnumerateDirectories(dir))
                ScanDirectory(subDir, typeSet, results, excludePaths, excludePatterns, maxDepth, depth + 1, maxFiles);
        }
        catch (UnauthorizedAccessException) { }
        catch (PathTooLongException) { }
    }

    private static bool MatchPattern(string name, string pattern)
    {
        if (pattern.StartsWith("*.")) return name.EndsWith(pattern[1..], StringComparison.OrdinalIgnoreCase);
        if (pattern.StartsWith("*")) return name.EndsWith(pattern[1..], StringComparison.OrdinalIgnoreCase);
        if (pattern.EndsWith("*")) return name.StartsWith(pattern[..^1], StringComparison.OrdinalIgnoreCase);
        return name.Equals(pattern, StringComparison.OrdinalIgnoreCase);
    }

    private static string GetFileIcon(string ext)
    {
        ext = ext.StartsWith(".") ? ext[1..].ToLowerInvariant() : ext.ToLowerInvariant();
        return ext switch
        {
            "exe" => "\U0001F4E6",
            "lnk" => "\U0001F517",
            "bat" => "\U0001F4DC",
            "ps1" => "⚡",
            "txt" => "\U0001F4C4",
            "pdf" => "\U0001F4D5",
            "png" => "\U0001F5BC️",
            "jpg" => "\U0001F5BC️",
            _ => "\U0001F4C4"
        };
    }

    private static int GetExtOrder(string ext)
    {
        ext = ext.ToLowerInvariant();
        return ext switch
        {
            ".exe" => 1,
            ".lnk" => 2,
            _ => 99
        };
    }
}
