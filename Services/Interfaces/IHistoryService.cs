namespace Quera.Services;

public interface IHistoryService
{
    void Record(string query);
    int GetFrequency(string text);
    void RecordSelection(string text);
}
