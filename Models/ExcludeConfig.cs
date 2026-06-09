namespace Quera.Models;

public class ExcludeConfig
{
    public List<string> Paths { get; set; } = new();
    public List<string> Patterns { get; set; } = new();
}
