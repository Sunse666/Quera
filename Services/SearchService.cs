namespace Quera.Services;

public class SearchService : ISearchService
{
    private readonly IFileIndexService _fileIndexService;
    private readonly ILogger<SearchService> _logger;
    private readonly IConfigService? _configService;

    private readonly IHistoryService _history;

    public SearchService(IFileIndexService fileIndexService, ILogger<SearchService> logger, IConfigService configService, IHistoryService history)
    {
        _fileIndexService = fileIndexService; _logger = logger;
        _configService = configService; _history = history;
    }

    public record SearchResultContainer(List<SearchResult> Items, int TotalCount);

    public SearchResultContainer Search(string query, ConfigData config, int maxResults)
    {
        var results = new List<SearchResult>();
        query = query.Trim();
        if (query.Length == 0) return new(new(), 0);

        var sys = MatchSystem(query);
        if (sys != null) { results.Add(sys); return new(results, 1); }

        var calc = TryCalc(query);
        if (calc != null) results.Add(calc);

        var alias = config.Aliases.FirstOrDefault(a => string.Equals(a.Keyword, query, StringComparison.OrdinalIgnoreCase));
        if (alias != null)
            results.Add(Make($"别名: {alias.Keyword} → {alias.Action}", alias.Action, SearchResultType.Command, "🔀",
                action: alias.Action, admin: alias.Admin));

        var lowerQuery = query.ToLowerInvariant();
        var prio = config.Priority;

        char? prefix = query.Length > 1 ? query[0] : null;
        var realQuery = prefix is '>' or '/' or '@' ? query[1..].TrimStart() : query;
        var realLower = realQuery.ToLowerInvariant();

        var spacePos = realQuery.IndexOf(' ');
        if (spacePos > 0)
        {
            var kw = realQuery[..spacePos];
            var sq = realQuery[(spacePos + 1)..].Trim();
            if (kw.Length > 0 && sq.Length > 0)
            {
                foreach (var engine in config.SearchEngines)
                {
                    if ((engine.Keyword ?? "").ToLowerInvariant() == kw.ToLowerInvariant())
                    {
                        var url = (engine.Url ?? "").Replace("{query}", Uri.EscapeDataString(sq));
                        results.Add(new SearchResult { Name = $"{(engine.Name ?? "搜索")}: {sq}",
                            Description = url, Type = SearchResultType.WebSearch,
                            Icon = engine.Icon ?? "\U0001F50D", Url = url });
                        return new(results, results.Count);
                    }
                }
            }
        }

        if (prefix != '>')
        {
            foreach (var bm in config.Bookmarks)
            {
                if (IsMatch(bm.Keyword, realLower) || IsMatch(bm.Name, realLower))
                    results.Add(Make(bm.Name ?? "", bm.Url ?? "", SearchResultType.Bookmark, bm.Icon ?? "\U0001F517", url: bm.Url ?? ""));
            }

            foreach (var file in _fileIndexService.Cache)
            {
                if (IsMatch(file.Name, realLower) || IsMatch(file.Description, realLower))
                    results.Add(Make(file.Name, file.Description, file.Type, file.Icon, path: file.Path, ext: file.Extension, source: file.Source));
            }

            foreach (var folder in config.Folders)
            {
                if (IsMatch(folder.Keyword, realLower) || IsMatch(folder.Name, realLower))
                    results.Add(Make(folder.Name ?? "", ConfigServiceHelper.ExpandPathStatic(folder.Path ?? ""),
                        SearchResultType.Folder, folder.Icon ?? "\U0001F4C1", path: folder.Path ?? ""));
            }
        }

        if (prefix != '/' && prefix != '@')
        {
            foreach (var cmd in config.Commands)
            {
                if (IsMatch(cmd.Keyword, realLower) || IsMatch(cmd.Name, realLower))
                    results.Add(Make(cmd.Name ?? "", cmd.Keyword ?? "", SearchResultType.Command, cmd.Icon ?? "⚡", action: cmd.Action ?? "", admin: cmd.Admin));
            }
        }

        foreach (var engine in config.SearchEngines)
        {
            if (IsMatch(engine.Keyword, realLower) || IsMatch(engine.Name, realLower))
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
            r.SortOrder = Rank(r.Type) - _history.GetFrequency(r.Name + r.Description) / 10;
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

    private static SearchResult? MatchSystem(string query)
    {
        var q = query.ToLowerInvariant();
        return q switch
        {
            "shutdown" or "关机" => Make("关机", "shutdown /s /t 0", SearchResultType.Command, "🛑", action: "cmd:shutdown /s /t 0"),
            "restart" or "重启" => Make("重启", "shutdown /r /t 0", SearchResultType.Command, "🔄", action: "cmd:shutdown /r /t 0"),
            "lock" or "锁定" => Make("锁定屏幕", "rundll32.exe user32.dll,LockWorkStation", SearchResultType.Command, "🔒", action: "run:rundll32.exe user32.dll,LockWorkStation"),
            "sleep" or "休眠" => Make("休眠", "rundll32.exe powrprof.dll,SetSuspendState 0,1,0", SearchResultType.Command, "💤", action: "run:rundll32.exe powrprof.dll,SetSuspendState 0,1,0"),
            _ => null
        };
    }

    private static SearchResult? TryCalc(string query)
    {
        if (query.Length == 0 || query[0] < '0' || query[0] > '9') return null;
        try
        {
            var expr = query.Replace("×", "*").Replace("÷", "/").Replace("x", "*").Replace("^", "**");
            if (!expr.Any(c => "+-*/%".Contains(c))) return null;
            var dt = new System.Data.DataTable();
            var result = dt.Compute(expr, null);
            return Make($"= {result}", query, SearchResultType.Command, "🧮", action: result.ToString() ?? "");
        }
        catch { return null; }
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
