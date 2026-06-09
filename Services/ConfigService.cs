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
        _configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config.ini");
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
                path[1..].TrimStart('\\', '/'));

        if (path.Contains(':') || path.StartsWith("\\\\"))
            return path;

        return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, path);
    }

    private ConfigData Parse()
    {
        var data = new ConfigData();

        if (!File.Exists(_configPath)) return data;

        var text = File.ReadAllText(_configPath);

        if (text.Length == 0) return data;

        if (text.StartsWith("﻿"))
            text = text[1..];

        text = text.Replace("\r\n", "\n").Replace("\r", "\n");

        var lines = text.Split('\n');

        string? currentSection = null;
        Dictionary<string, object>? currentItem = null;
        bool fileTypeCleared = false;

        foreach (var rawLine in lines)
        {
            var line = rawLine.Trim();
            if (line.Length == 0) continue;
            if (line[0] == '#' || line[0] == ';') continue;

            if (line[0] == '[' && line[^1] == ']')
            {
                var section = line[1..^1].Trim().ToLowerInvariant();
                currentItem = null;

                switch (section)
                {
                    case "command":
                        currentItem = new Dictionary<string, object>();
                        data.Commands.Add(new CommandItem());
                        currentSection = "command";
                        break;
                    case "folder":
                        currentItem = new Dictionary<string, object>();
                        data.Folders.Add(new FolderItem());
                        currentSection = "folder";
                        break;
                    case "bookmark":
                        currentItem = new Dictionary<string, object>();
                        data.Bookmarks.Add(new BookmarkItem());
                        currentSection = "bookmark";
                        break;
                    case "search":
                        currentItem = new Dictionary<string, object>();
                        data.WebSearchEngines.Add(new SearchEngine());
                        currentSection = "search";
                        break;
                    case "filetype":
                        if (!fileTypeCleared)
                        {
                            data.FileTypes.Clear();
                            fileTypeCleared = true;
                        }
                        currentSection = "filetype";
                        break;
                    default:
                        currentSection = section;
                        break;
                }
                continue;
            }

            if (currentSection == "path")
            {
                var path = Clean(line);
                if (path.Length > 0)
                    data.SearchPaths.Add(path);
                continue;
            }

            if (currentSection == "filetype")
            {
                var ext = Clean(line);
                if (ext.Length > 0)
                {
                    if (!ext.StartsWith(".")) ext = "." + ext;
                    data.FileTypes.Add(ext);
                }
                continue;
            }

            var eqPos = line.IndexOf('=');
            if (eqPos < 0) continue;

            var key = line[..eqPos].Trim().ToLowerInvariant();
            var value = Clean(line[(eqPos + 1)..]);

            if (key.Length == 0) continue;

            if (currentSection == "settings")
            {
                switch (key)
                {
                    case "hotkey": data.Hotkey = value; break;
                    case "width": data.Width = int.TryParse(value, out var w) ? w : 680; break;
                    case "height": data.Height = int.TryParse(value, out var h) ? h : 480; break;
                    case "opacity": data.Opacity = double.TryParse(value, out var o) ? o / 100.0 : 0.95; break;
                    case "max_results": data.MaxResults = int.TryParse(value, out var m) ? m : 30; break;
                    case "autostart": data.AutoStart = value == "true"; break;
                }
            }
            else if (currentSection == "priority")
            {
                switch (key)
                {
                    case "types":
                        data.Priority.Types = value.Split(',')
                            .Select(s => s.Trim()).Where(s => s.Length > 0).ToList();
                        break;
                    case "extensions":
                        data.Priority.Extensions = value.Split(',')
                            .Select(s => s.Trim()).Where(s => s.Length > 0)
                            .Select(s => s.StartsWith(".") ? s : "." + s).ToList();
                        break;
                    case "custom_path_first":
                        data.Priority.CustomPathFirst = value == "true";
                        break;
                }
            }
            else if (currentSection == "command" && data.Commands.Count > 0)
            {
                var cmd = data.Commands[^1];
                SetProperty(cmd, key, value);
            }
            else if (currentSection == "folder" && data.Folders.Count > 0)
            {
                var folder = data.Folders[^1];
                SetProperty(folder, key, value);
            }
            else if (currentSection == "bookmark" && data.Bookmarks.Count > 0)
            {
                var bm = data.Bookmarks[^1];
                SetProperty(bm, key, value);
            }
            else if (currentSection == "search" && data.WebSearchEngines.Count > 0)
            {
                var engine = data.WebSearchEngines[^1];
                SetProperty(engine, key, value);
            }
        }

        return data;
    }

    private static void SetProperty(object obj, string key, string value)
    {
        switch (obj)
        {
            case CommandItem cmd:
                switch (key)
                {
                    case "name": cmd.Name = value; break;
                    case "keyword": cmd.Keyword = value; break;
                    case "action": cmd.Action = value; break;
                    case "icon": cmd.Icon = value; break;
                    case "admin": cmd.Admin = value == "true"; break;
                }
                break;
            case FolderItem folder:
                switch (key)
                {
                    case "name": folder.Name = value; break;
                    case "keyword": folder.Keyword = value; break;
                    case "path": folder.Path = value; break;
                    case "icon": folder.Icon = value; break;
                }
                break;
            case BookmarkItem bm:
                switch (key)
                {
                    case "name": bm.Name = value; break;
                    case "keyword": bm.Keyword = value; break;
                    case "url": bm.Url = value; break;
                    case "icon": bm.Icon = value; break;
                }
                break;
            case SearchEngine se:
                switch (key)
                {
                    case "name": se.Name = value; break;
                    case "keyword": se.Keyword = value; break;
                    case "url": se.Url = value; break;
                    case "icon": se.Icon = value; break;
                }
                break;
        }
    }

    private static string Clean(string s)
    {
        if (string.IsNullOrEmpty(s)) return "";
        s = s.Trim();
        if (s.Length >= 2)
        {
            var first = s[0];
            var last = s[^1];
            if ((first == '"' && last == '"') || (first == '\'' && last == '\''))
                return s[1..^1];
        }
        return s;
    }
}
