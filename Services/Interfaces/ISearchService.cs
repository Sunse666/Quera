namespace Quera.Services;

public interface ISearchService
{
    SearchService.SearchResultContainer Search(string query, ConfigData config, int maxResults);
}
