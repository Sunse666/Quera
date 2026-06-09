namespace Quera.Services;

public class ExecutionService : IExecutionService
{
    private readonly ILogger<ExecutionService> _logger;

    public ExecutionService(ILogger<ExecutionService> logger)
    {
        _logger = logger;
    }

    public string? Execute(SearchResult item)
    {
        if (item == null) return null;

        return item.Type switch
        {
            SearchResultType.Command => ExecuteCommand(item),
            SearchResultType.File or SearchResultType.App => ExecuteFile(item),
            SearchResultType.Folder => ExecuteFolder(item),
            SearchResultType.Bookmark or SearchResultType.WebSearch => ExecuteUrl(item),
            SearchResultType.SearchHint => (item.Keyword ?? "") + " ",
            _ => null
        };
    }

    private string? ExecuteCommand(SearchResult item)
    {
        var action = item.Action ?? "";

        try
        {
            if (action.StartsWith("shell:"))
            {
                var target = action[6..];
                StartProcess("explorer.exe", target, item.IsAdmin);
            }
            else if (action.StartsWith("cmd:"))
            {
                var cmd = action[4..];
                StartProcess("cmd.exe", "/c " + cmd, item.IsAdmin);
            }
            else if (action.StartsWith("ps:"))
            {
                var psCmd = action[3..];
                StartProcess("powershell.exe", "-Command " + psCmd, item.IsAdmin);
            }
            else if (action.StartsWith("run:"))
            {
                var program = action[4..];
                StartProcess(program, null, item.IsAdmin);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to execute command: {Action}", action);
        }

        return null;
    }

    private static string? ExecuteFile(SearchResult item)
    {
        if (item.Path == null) return null;
        try
        {
            Process.Start(new ProcessStartInfo(item.Path) { UseShellExecute = true });
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Failed to open file: {ex.Message}");
        }
        return null;
    }

    private static string? ExecuteFolder(SearchResult item)
    {
        if (item.Path == null) return null;
        try
        {
            Process.Start("explorer.exe", item.Path);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Failed to open folder: {ex.Message}");
        }
        return null;
    }

    private static string? ExecuteUrl(SearchResult item)
    {
        if (item.Url == null) return null;
        try
        {
            Process.Start(new ProcessStartInfo(item.Url) { UseShellExecute = true });
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Failed to open URL: {ex.Message}");
        }
        return null;
    }

    private static void StartProcess(string file, string? args, bool admin)
    {
        var psi = new ProcessStartInfo(file, args ?? "")
        {
            UseShellExecute = true,
            Verb = admin ? "runas" : "open"
        };
        Process.Start(psi);
    }
}
