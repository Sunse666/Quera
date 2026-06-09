namespace Quera.Models;

public class UIConfig
{
    public int BorderRadius { get; set; } = 20;
    public string FontFamily { get; set; } = "Microsoft YaHei UI";
    public int FontSizeSearch { get; set; } = 17;
    public int FontSizeResultName { get; set; } = 14;
    public int FontSizeResultDesc { get; set; } = 11;
    public int ItemHeight { get; set; } = 44;
    public bool ShowIcons { get; set; } = true;
    public bool ShowTypeBadge { get; set; } = true;
    public int MaxVisibleItems { get; set; } = 10;
    public bool ShowStatusBar { get; set; } = true;
}
