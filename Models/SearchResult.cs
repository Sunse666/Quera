namespace Quera.Models;

public class SearchResult
{
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public SearchResultType Type { get; set; }
    public string Icon { get; set; } = "\U0001F4C4";
    public string? Path { get; set; }
    public string? Url { get; set; }
    public string? Action { get; set; }
    public bool IsAdmin { get; set; }
    public string? Keyword { get; set; }
    public string? Extension { get; set; }
    public string? Source { get; set; }
    public int SortOrder { get; set; } = 3;
    public int ExtOrder { get; set; } = 99;
}
