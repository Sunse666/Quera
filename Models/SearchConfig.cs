namespace Quera.Models;

public class SearchConfig
{
    public string MatchMode { get; set; } = "contains";
    public bool IncludeDirectories { get; set; }
    public int MaxDepth { get; set; } = -1;
}
