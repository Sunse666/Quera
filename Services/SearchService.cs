namespace Quera.Services;

public class SearchService : ISearchService
{
    private readonly IFileIndexService _fileIndexService;
    private readonly ILogger<SearchService> _logger;
    private readonly IConfigService? _configService;

    public SearchService(IFileIndexService fileIndexService, ILogger<SearchService> logger, IConfigService configService)
    {
        _fileIndexService = fileIndexService;
        _logger = logger;
        _configService = configService;
    }

    public record SearchResultContainer(List<SearchResult> Items, int TotalCount);

    public SearchResultContainer Search(string query, ConfigData config, int maxResults)
    {
        var results = new List<SearchResult>();
        query = query.Trim();
        if (query.Length == 0) return new(new(), 0);

        var lowerQuery = query.ToLowerInvariant();
        var prio = config.Priority;

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
                            Description = url, Type = SearchResultType.WebSearch,
                            Icon = engine.Icon ?? "\U0001F50D", Url = url
                        });
                        return new(results, results.Count);
                    }
                }
            }
        }

        foreach (var cmd in config.Commands)
        {
            if (IsMatch(cmd.Keyword, lowerQuery) || IsMatch(cmd.Name, lowerQuery))
                results.Add(Make(cmd.Name ?? "", cmd.Keyword ?? "", SearchResultType.Command, cmd.Icon ?? "⚡",
                    action: cmd.Action ?? "", admin: cmd.Admin));
        }

        foreach (var bm in config.Bookmarks)
        {
            if (IsMatch(bm.Keyword, lowerQuery) || IsMatch(bm.Name, lowerQuery))
                results.Add(Make(bm.Name ?? "", bm.Url ?? "", SearchResultType.Bookmark, bm.Icon ?? "\U0001F517",
                    url: bm.Url ?? ""));
        }

        foreach (var file in _fileIndexService.Cache)
        {
            if (IsMatch(file.Name, lowerQuery) || IsMatch(file.Description, lowerQuery))
                results.Add(Make(file.Name, file.Description, file.Type, file.Icon,
                    path: file.Path, ext: file.Extension, source: file.Source));
        }

        foreach (var folder in config.Folders)
        {
            if (IsMatch(folder.Keyword, lowerQuery) || IsMatch(folder.Name, lowerQuery))
                results.Add(Make(folder.Name ?? "", ConfigServiceHelper.ExpandPathStatic(folder.Path ?? ""),
                    SearchResultType.Folder, folder.Icon ?? "\U0001F4C1", path: folder.Path ?? ""));
        }

        foreach (var engine in config.SearchEngines)
        {
            if (IsMatch(engine.Keyword, lowerQuery) || IsMatch(engine.Name, lowerQuery))
                results.Add(Make(engine.Name ?? "", $"输入 \"{engine.Keyword ?? ""} 关键词\" 进行搜索",
                    SearchResultType.SearchHint, engine.Icon ?? "\U0001F50D", keyword: engine.Keyword ?? ""));
        }

        var typeRank = prio.Types.ConvertAll(t => t.ToLowerInvariant());
        var extRank = prio.Extensions.ConvertAll(e => e.StartsWith(".") ? e.ToLowerInvariant() : "." + e.ToLowerInvariant());

        int Rank(SearchResultType t) => t switch
        {
            SearchResultType.Command => typeRank.IndexOf("command"),
            SearchResultType.Bookmark => typeRank.IndexOf("bookmark"),
            SearchResultType.File or SearchResultType.App => typeRank.IndexOf("file"),
            SearchResultType.Folder => typeRank.IndexOf("folder"),
            SearchResultType.WebSearch => typeRank.IndexOf("search"),
            SearchResultType.SearchHint => typeRank.IndexOf("search_hint"),
            _ => 99
        };

        int ExtOrd(string? e) => e == null ? 99 : extRank.IndexOf(e.ToLowerInvariant());

        foreach (var r in results)
        {
            r.SortOrder = Rank(r.Type);
            r.ExtOrder = ExtOrd(r.Extension);
        }

        results.Sort((a, b) =>
        {
            int cmp = a.SortOrder.CompareTo(b.SortOrder);
            if (cmp != 0) return cmp;
            return a.ExtOrder.CompareTo(b.ExtOrder);
        });

        var totalCount = results.Count;
        maxResults = maxResults > 0 ? maxResults : 30;
        if (results.Count > maxResults)
            results = results.Take(maxResults).ToList();

        return new(results, totalCount);
    }

    private static SearchResult Make(string name, string desc, SearchResultType type, string icon,
        string? path = null, string? url = null, string? action = null, string? ext = null,
        string? source = null, string? keyword = null, bool admin = false)
    {
        return new SearchResult
        {
            Name = name, Description = desc, Type = type, Icon = icon,
            Path = path, Url = url, Action = action, Extension = ext,
            Source = source, Keyword = keyword, IsAdmin = admin
        };
    }

    private bool IsMatch(string? text, string lowerQuery)
    {
        if (string.IsNullOrEmpty(text) || string.IsNullOrEmpty(lowerQuery)) return false;
        var t = text.ToLowerInvariant();
        var mode = _configService?.Current?.Search?.MatchMode ?? "contains";
        return mode switch
        {
            "starts_with" => t.StartsWith(lowerQuery),
            "fuzzy" => FuzzyMatch(t, lowerQuery),
            _ => t.Contains(lowerQuery),
        };
    }

    private static bool FuzzyMatch(string text, string query)
    {
        int qi = 0;
        for (int ti = 0; ti < text.Length && qi < query.Length; ti++)
            if (text[ti] == query[qi]) qi++;
        return qi == query.Length;
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
