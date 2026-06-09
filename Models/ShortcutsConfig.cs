namespace Quera.Models;

public class ShortcutsConfig
{
    public string NextPage { get; set; } = "Tab";
    public string PrevPage { get; set; } = "Shift+Tab";
    public string Execute { get; set; } = "Enter";
    public string Hide { get; set; } = "Escape";
    public string OpenConfig { get; set; } = "Ctrl+,";
    public string ReloadConfig { get; set; } = "Ctrl+R";
}
