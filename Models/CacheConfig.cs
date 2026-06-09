namespace Quera.Models;

public class CacheConfig
{
    public bool Enabled { get; set; } = true;
    public bool RefreshOnStart { get; set; } = true;
    public int MaxFiles { get; set; } = 50000;
}
