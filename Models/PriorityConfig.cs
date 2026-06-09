namespace Quera.Models;

public class PriorityConfig
{
    public List<string> Types { get; set; } = new() { "command", "bookmark", "file", "folder", "search", "search_hint" };
    public List<string> Extensions { get; set; } = new() { ".exe", ".lnk", ".bat", ".ps1" };
    public bool CustomPathFirst { get; set; } = true;
}
