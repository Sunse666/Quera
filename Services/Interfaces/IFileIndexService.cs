namespace Quera.Services;

public interface IFileIndexService
{
    bool CacheBuilt { get; }
    List<SearchResult> Cache { get; }
    Task BuildCacheAsync(List<string> searchPaths, List<string> fileTypes);
}
