namespace Quera.Services;

public class HistoryService : IHistoryService
{
    private readonly Dictionary<string, int> _freq = new(StringComparer.OrdinalIgnoreCase);
    private const int MaxEntries = 1000;

    public void Record(string query)
    {
        var q = query.Trim().ToLowerInvariant();
        if (q.Length == 0) return;
        if (_freq.ContainsKey(q))
            _freq[q]++;
        else
        {
            if (_freq.Count >= MaxEntries) return;
            _freq[q] = 1;
        }
    }

    public int GetFrequency(string text)
    {
        var q = text.Trim().ToLowerInvariant();
        return _freq.TryGetValue(q, out var f) ? f : 0;
    }

    public void RecordSelection(string selectedText)
    {
        Record(selectedText);
    }
}
