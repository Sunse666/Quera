using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Quera.Services;

public class ConfigService : IConfigService
{
    private readonly ILogger<ConfigService> _logger;
    private string _configPath = "";

    public ConfigData Current { get; private set; } = new();

    public ConfigService(ILogger<ConfigService> logger)
    {
        _logger = logger;
    }

    public void Load()
    {
        _configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config.yaml");
        Current = Parse();
    }

    public void Reload()
    {
        Current = Parse();
    }

    public string ExpandPath(string path)
    {
        if (string.IsNullOrWhiteSpace(path)) return "";
        if (path.StartsWith("~"))
            return Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                path[1..].TrimStart('\\', '/').Replace('/', '\\'));
        if (path.Contains(':') || path.StartsWith("\\\\"))
            return path;
        return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, path);
    }

    private ConfigData Parse()
    {
        try
        {
            if (!File.Exists(_configPath))
                return new ConfigData();

            var yaml = File.ReadAllText(_configPath);
            if (string.IsNullOrWhiteSpace(yaml))
                return new ConfigData();

            var deserializer = new DeserializerBuilder()
                .WithNamingConvention(UnderscoredNamingConvention.Instance)
                .IgnoreUnmatchedProperties()
                .Build();

            var raw = deserializer.Deserialize<YamlConfig>(yaml);
            return Map(raw);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to parse config, using defaults");
            return new ConfigData();
        }
    }

    private static ConfigData Map(YamlConfig raw)
    {
        var cfg = new ConfigData();

        if (raw.Settings != null)
        {
            cfg.Hotkey = raw.Settings.Hotkey ?? cfg.Hotkey;
            cfg.Width = raw.Settings.Width ?? cfg.Width;
            cfg.Opacity = (raw.Settings.Opacity ?? 96) / 100.0;
            cfg.MaxResults = raw.Settings.MaxResults ?? cfg.MaxResults;
            cfg.AutoStart = raw.Settings.AutoStart ?? cfg.AutoStart;
            cfg.HideOnDeactivate = raw.Settings.HideOnDeactivate ?? cfg.HideOnDeactivate;
            cfg.HideDelayMs = raw.Settings.HideDelayMs ?? cfg.HideDelayMs;
            cfg.ShowOnStartup = raw.Settings.ShowOnStartup ?? cfg.ShowOnStartup;
        }

        if (raw.Search != null)
        {
            cfg.Search.MatchMode = raw.Search.MatchMode ?? cfg.Search.MatchMode;
            cfg.Search.IncludeDirectories = raw.Search.IncludeDirectories ?? cfg.Search.IncludeDirectories;
            cfg.Search.MaxDepth = raw.Search.MaxDepth ?? cfg.Search.MaxDepth;
        }

        if (raw.Exclude != null)
        {
            if (raw.Exclude.Paths != null) cfg.Exclude.Paths = raw.Exclude.Paths;
            if (raw.Exclude.Patterns != null) cfg.Exclude.Patterns = raw.Exclude.Patterns;
        }

        if (raw.Cache != null)
        {
            cfg.Cache.Enabled = raw.Cache.Enabled ?? cfg.Cache.Enabled;
            cfg.Cache.RefreshOnStart = raw.Cache.RefreshOnStart ?? cfg.Cache.RefreshOnStart;
            cfg.Cache.MaxFiles = raw.Cache.MaxFiles ?? cfg.Cache.MaxFiles;
        }

        if (raw.Ui != null)
        {
            cfg.UI.BorderRadius = raw.Ui.BorderRadius ?? cfg.UI.BorderRadius;
            cfg.UI.FontFamily = raw.Ui.FontFamily ?? cfg.UI.FontFamily;
            cfg.UI.FontSizeSearch = raw.Ui.FontSizeSearch ?? cfg.UI.FontSizeSearch;
            cfg.UI.FontSizeResultName = raw.Ui.FontSizeResultName ?? cfg.UI.FontSizeResultName;
            cfg.UI.FontSizeResultDesc = raw.Ui.FontSizeResultDesc ?? cfg.UI.FontSizeResultDesc;
            cfg.UI.ItemHeight = raw.Ui.ItemHeight ?? cfg.UI.ItemHeight;
            cfg.UI.ShowIcons = raw.Ui.ShowIcons ?? cfg.UI.ShowIcons;
            cfg.UI.ShowTypeBadge = raw.Ui.ShowTypeBadge ?? cfg.UI.ShowTypeBadge;
            cfg.UI.ShowStatusBar = raw.Ui.ShowStatusBar ?? cfg.UI.ShowStatusBar;
            cfg.UI.MaxVisibleItems = raw.Ui.MaxVisibleItems ?? cfg.UI.MaxVisibleItems;
        }

        if (raw.Terminal != null)
        {
            cfg.Terminal.Default = raw.Terminal.Default ?? cfg.Terminal.Default;
            cfg.Terminal.AdminDefault = raw.Terminal.AdminDefault ?? cfg.Terminal.AdminDefault;
        }

        if (raw.Window != null)
        {
            cfg.Window.AlwaysOnTop = raw.Window.AlwaysOnTop ?? cfg.Window.AlwaysOnTop;
            cfg.Window.CornerRadius = raw.Window.CornerRadius ?? cfg.Window.CornerRadius;
        }

        if (raw.Icons != null)
        {
            cfg.Icons.EnableImageIcons = raw.Icons.EnableImageIcons ?? cfg.Icons.EnableImageIcons;
            cfg.Icons.CacheIcons = raw.Icons.CacheIcons ?? cfg.Icons.CacheIcons;
        }

        if (raw.SearchBox != null)
        {
            cfg.SearchBox.Placeholder = raw.SearchBox.Placeholder ?? cfg.SearchBox.Placeholder;
            cfg.SearchBox.Icon = raw.SearchBox.Icon ?? cfg.SearchBox.Icon;
            cfg.SearchBox.EscHint = raw.SearchBox.EscHint ?? cfg.SearchBox.EscHint;
        }

        if (raw.Results != null)
        {
            cfg.Results.PaddingH = raw.Results.PaddingH ?? cfg.Results.PaddingH;
            cfg.Results.PaddingV = raw.Results.PaddingV ?? cfg.Results.PaddingV;
            cfg.Results.Margin = raw.Results.Margin ?? cfg.Results.Margin;
            cfg.Results.IconSize = raw.Results.IconSize ?? cfg.Results.IconSize;
            cfg.Results.BadgeFontSize = raw.Results.BadgeFontSize ?? cfg.Results.BadgeFontSize;
        }

        if (raw.Layout != null)
        {
            cfg.Layout.OuterMargin = raw.Layout.OuterMargin ?? cfg.Layout.OuterMargin;
            cfg.Layout.CardGap = raw.Layout.CardGap ?? cfg.Layout.CardGap;
            cfg.Layout.SearchPadding = raw.Layout.SearchPadding ?? cfg.Layout.SearchPadding;
            cfg.Layout.ResultsPadding = raw.Layout.ResultsPadding ?? cfg.Layout.ResultsPadding;
        }

        if (raw.Shortcuts != null)
        {
            cfg.Shortcuts.NextPage = raw.Shortcuts.NextPage ?? cfg.Shortcuts.NextPage;
            cfg.Shortcuts.PrevPage = raw.Shortcuts.PrevPage ?? cfg.Shortcuts.PrevPage;
            cfg.Shortcuts.Execute = raw.Shortcuts.Execute ?? cfg.Shortcuts.Execute;
            cfg.Shortcuts.Hide = raw.Shortcuts.Hide ?? cfg.Shortcuts.Hide;
            cfg.Shortcuts.OpenConfig = raw.Shortcuts.OpenConfig ?? cfg.Shortcuts.OpenConfig;
            cfg.Shortcuts.ReloadConfig = raw.Shortcuts.ReloadConfig ?? cfg.Shortcuts.ReloadConfig;
        }

        if (raw.Colors != null)
        {
            cfg.Colors = new ColorConfig
            {
                Background = raw.Colors.Background ?? cfg.Colors.Background,
                SearchCard = raw.Colors.SearchCard ?? cfg.Colors.SearchCard,
                SearchBorder = raw.Colors.SearchBorder ?? cfg.Colors.SearchBorder,
                ResultCard = raw.Colors.ResultCard ?? cfg.Colors.ResultCard,
                ResultBorder = raw.Colors.ResultBorder ?? cfg.Colors.ResultBorder,
                ResultHover = raw.Colors.ResultHover ?? cfg.Colors.ResultHover,
                ResultSelectedStart = raw.Colors.ResultSelectedStart ?? cfg.Colors.ResultSelectedStart,
                ResultSelectedEnd = raw.Colors.ResultSelectedEnd ?? cfg.Colors.ResultSelectedEnd,
                TextPrimary = raw.Colors.TextPrimary ?? cfg.Colors.TextPrimary,
                TextSecondary = raw.Colors.TextSecondary ?? cfg.Colors.TextSecondary,
                TextMuted = raw.Colors.TextMuted ?? cfg.Colors.TextMuted,
                Accent = raw.Colors.Accent ?? cfg.Colors.Accent,
            };
        }

        if (raw.Paths != null) cfg.SearchPaths = raw.Paths;
        if (raw.FileTypes != null) cfg.FileTypes = raw.FileTypes.Select(t => t.StartsWith(".") ? t : "." + t).ToList();
        if (raw.Commands != null) cfg.Commands = raw.Commands;
        if (raw.Bookmarks != null) cfg.Bookmarks = raw.Bookmarks;
        if (raw.Folders != null) cfg.Folders = raw.Folders;
        if (raw.SearchEngines != null) cfg.SearchEngines = raw.SearchEngines;

        if (raw.Priority != null)
        {
            if (raw.Priority.Types != null) cfg.Priority.Types = raw.Priority.Types;
            if (raw.Priority.Extensions != null) cfg.Priority.Extensions = raw.Priority.Extensions.Select(t => t.StartsWith(".") ? t : "." + t).ToList();
            if (raw.Priority.CustomPathFirst != null) cfg.Priority.CustomPathFirst = raw.Priority.CustomPathFirst.Value;
        }

        return cfg;
    }

    private class YamlConfig
    {
        public YamlSettings? Settings { get; set; }
        public YamlColors? Colors { get; set; }
        public YamlWindow? Window { get; set; }
        public YamlIcon? Icons { get; set; }
        public YamlSearchBox? SearchBox { get; set; }
        public YamlResults? Results { get; set; }
        public YamlLayout? Layout { get; set; }
        public YamlShortcuts? Shortcuts { get; set; }
        public YamlSearch? Search { get; set; }
        public YamlExclude? Exclude { get; set; }
        public YamlCache? Cache { get; set; }
        public YamlUI? Ui { get; set; }
        public YamlTerminal? Terminal { get; set; }
        public List<string>? Paths { get; set; }
        public List<string>? FileTypes { get; set; }
        public List<CommandItem>? Commands { get; set; }
        public List<BookmarkItem>? Bookmarks { get; set; }
        public List<FolderItem>? Folders { get; set; }
        public List<SearchEngine>? SearchEngines { get; set; }
        public YamlPriority? Priority { get; set; }
    }

    private class YamlSettings
    {
        public string? Hotkey { get; set; }
        public int? Width { get; set; }
        public int? Opacity { get; set; }
        public int? MaxResults { get; set; }
        public bool? AutoStart { get; set; }
        public bool? HideOnDeactivate { get; set; }
        public int? HideDelayMs { get; set; }
        public bool? ShowOnStartup { get; set; }
    }

    private class YamlColors
    {
        public string? Background { get; set; }
        public string? SearchCard { get; set; }
        public string? SearchBorder { get; set; }
        public string? ResultCard { get; set; }
        public string? ResultBorder { get; set; }
        public string? ResultHover { get; set; }
        public string? ResultSelectedStart { get; set; }
        public string? ResultSelectedEnd { get; set; }
        public string? TextPrimary { get; set; }
        public string? TextSecondary { get; set; }
        public string? TextMuted { get; set; }
        public string? Accent { get; set; }
    }

    private class YamlSearch
    {
        public string? MatchMode { get; set; }
        public bool? IncludeDirectories { get; set; }
        public int? MaxDepth { get; set; }
    }

    private class YamlExclude
    {
        public List<string>? Paths { get; set; }
        public List<string>? Patterns { get; set; }
    }

    private class YamlCache
    {
        public bool? Enabled { get; set; }
        public bool? RefreshOnStart { get; set; }
        public int? MaxFiles { get; set; }
    }

    private class YamlUI
    {
        public int? BorderRadius { get; set; }
        public string? FontFamily { get; set; }
        public int? FontSizeSearch { get; set; }
        public int? FontSizeResultName { get; set; }
        public int? FontSizeResultDesc { get; set; }
        public int? ItemHeight { get; set; }
        public bool? ShowIcons { get; set; }
        public bool? ShowTypeBadge { get; set; }
        public bool? ShowStatusBar { get; set; }
        public int? MaxVisibleItems { get; set; }
    }

    private class YamlTerminal
    {
        public string? Default { get; set; }
        public string? AdminDefault { get; set; }
    }

    private class YamlWindow
    {
        public bool? AlwaysOnTop { get; set; }
        public int? CornerRadius { get; set; }
    }

    private class YamlIcon
    {
        public bool? EnableImageIcons { get; set; }
        public bool? CacheIcons { get; set; }
    }

    private class YamlSearchBox
    {
        public string? Placeholder { get; set; }
        public string? Icon { get; set; }
        public string? EscHint { get; set; }
    }

    private class YamlResults
    {
        public int? PaddingH { get; set; }
        public int? PaddingV { get; set; }
        public int? Margin { get; set; }
        public int? IconSize { get; set; }
        public int? BadgeFontSize { get; set; }
    }

    private class YamlLayout
    {
        public int? OuterMargin { get; set; }
        public int? CardGap { get; set; }
        public int? SearchPadding { get; set; }
        public int? ResultsPadding { get; set; }
    }

    private class YamlShortcuts
    {
        public string? NextPage { get; set; }
        public string? PrevPage { get; set; }
        public string? Execute { get; set; }
        public string? Hide { get; set; }
        public string? OpenConfig { get; set; }
        public string? ReloadConfig { get; set; }
    }

    private class YamlPriority
    {
        public List<string>? Types { get; set; }
        public List<string>? Extensions { get; set; }
        public bool? CustomPathFirst { get; set; }
    }
}
