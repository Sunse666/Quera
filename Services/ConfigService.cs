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

    private class YamlPriority
    {
        public List<string>? Types { get; set; }
        public List<string>? Extensions { get; set; }
        public bool? CustomPathFirst { get; set; }
    }
}
