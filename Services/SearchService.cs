namespace Quera.Services;

public class SearchService : ISearchService
{
    private readonly IFileIndexService _fileIndexService;
    private readonly ILogger<SearchService> _logger;

    public SearchService(IFileIndexService fileIndexService, ILogger<SearchService> logger)
    {
        _fileIndexService = fileIndexService;
        _logger = logger;
    }

    public record SearchResultContainer(List<SearchResult> Items, int TotalCount);

    public SearchResultContainer Search(string query, ConfigData config, int maxResults)
    {
        var results = new List<SearchResult>();
        query = query.Trim();
        if (query.Length == 0) return new(new(), 0);

        var lowerQuery = query.ToLowerInvariant();

        var spacePos = query.IndexOf(' ');
        if (spacePos > 0)
        {
            var keyword = query[..spacePos];
            var searchQuery = query[(spacePos + 1)..].Trim();

            if (keyword.Length > 0 && searchQuery.Length > 0)
            {
                foreach (var engine in config.SearchEngines)
                {
                    if ((engine.Keyword ?? "").ToLowerInvariant() == keyword.ToLowerInvariant())
                    {
                        var url = (engine.Url ?? "").Replace("{query}", Uri.EscapeDataString(searchQuery));
                        results.Add(new SearchResult
                        {
                            Name = $"{(engine.Name ?? "搜索")}: {searchQuery}",
                            Description = url,
                            Type = SearchResultType.WebSearch,
                            Icon = engine.Icon ?? "\U0001F50D",
                            Url = url
                        });
                        return new(results, results.Count);
                    }
                }
            }
        }

        foreach (var cmd in config.Commands)
        {
            if (IsMatch(cmd.Keyword, lowerQuery) || IsMatch(cmd.Name, lowerQuery))
            {
                results.Add(new SearchResult
                {
                    Name = cmd.Name ?? "",
                    Description = cmd.Keyword ?? "",
                    Type = SearchResultType.Command,
                    Icon = cmd.Icon ?? "⚡",
                    Action = cmd.Action ?? "",
                    IsAdmin = cmd.Admin,
                    SortOrder = 2
                });
            }
        }

        foreach (var bookmark in config.Bookmarks)
        {
            if (IsMatch(bookmark.Keyword, lowerQuery) || IsMatch(bookmark.Name, lowerQuery))
            {
                results.Add(new SearchResult
                {
                    Name = bookmark.Name ?? "",
                    Description = bookmark.Url ?? "",
                    Type = SearchResultType.Bookmark,
                    Icon = bookmark.Icon ?? "\U0001F517",
                    Url = bookmark.Url ?? "",
                    SortOrder = 4
                });
            }
        }

        foreach (var file in _fileIndexService.Cache)
        {
            if (IsMatch(file.Name, lowerQuery) || IsMatch(file.Description, lowerQuery))
            {
                var order = file.Source == "custom" ? 1 : 3;
                results.Add(new SearchResult
                {
                    Name = file.Name,
                    Description = file.Description,
                    Type = file.Type,
                    Icon = file.Icon,
                    Path = file.Path,
                    Extension = file.Extension,
                    Source = file.Source,
                    SortOrder = order,
                    ExtOrder = file.ExtOrder
                });
            }
        }

        foreach (var folder in config.Folders)
        {
            if (IsMatch(folder.Keyword, lowerQuery) || IsMatch(folder.Name, lowerQuery))
            {
                var path = ConfigServiceHelper.ExpandPathStatic(folder.Path ?? "");
                results.Add(new SearchResult
                {
                    Name = folder.Name ?? "",
                    Description = path,
                    Type = SearchResultType.Folder,
                    Icon = folder.Icon ?? "\U0001F4C1",
                    Path = path,
                    SortOrder = 5
                });
            }
        }

        foreach (var engine in config.SearchEngines)
        {
            if (IsMatch(engine.Keyword, lowerQuery) || IsMatch(engine.Name, lowerQuery))
            {
                results.Add(new SearchResult
                {
                    Name = engine.Name ?? "",
                    Description = $"输入 \"{engine.Keyword ?? ""} 关键词\" 进行搜索",
                    Type = SearchResultType.SearchHint,
                    Icon = engine.Icon ?? "\U0001F50D",
                    Keyword = engine.Keyword ?? "",
                    SortOrder = 6
                });
            }
        }

        results.Sort((a, b) =>
        {
            var cmp = a.SortOrder.CompareTo(b.SortOrder);
            if (cmp != 0) return cmp;
            return a.ExtOrder.CompareTo(b.ExtOrder);
        });

        var totalCount = results.Count;
        maxResults = maxResults > 0 ? maxResults : 30;
        if (results.Count > maxResults)
            results = results.Take(maxResults).ToList();

        return new(results, totalCount);
    }

    private static bool IsMatch(string? text, string lowerQuery)
    {
        if (string.IsNullOrEmpty(text) || string.IsNullOrEmpty(lowerQuery)) return false;
        return text.ToLowerInvariant().Contains(lowerQuery);
    }
}

internal static class ConfigServiceHelper
{
    public static string ExpandPathStatic(string path)
    {
        if (string.IsNullOrWhiteSpace(path)) return "";
        if (path.StartsWith("~"))
            return Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                path[1..].TrimStart('\\', '/'));
        if (path.Contains(':') || path.StartsWith("\\\\")) return path;
        return Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, path));
    }
}
