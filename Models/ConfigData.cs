namespace Quera.Models;

public class ConfigData
{
    public string Hotkey { get; set; } = "Alt+Space";
    public int Width { get; set; } = 680;
    public double Opacity { get; set; } = 0.96;
    public int MaxResults { get; set; } = 30;
    public bool AutoStart { get; set; }
    public ColorConfig Colors { get; set; } = new();
    public List<string> SearchPaths { get; set; } = new();
    public List<string> FileTypes { get; set; } = new() { ".exe", ".lnk" };
    public PriorityConfig Priority { get; set; } = new();
    public List<CommandItem> Commands { get; set; } = new();
    public List<BookmarkItem> Bookmarks { get; set; } = new();
    public List<FolderItem> Folders { get; set; } = new();
    public List<SearchEngine> SearchEngines { get; set; } = new();
}
