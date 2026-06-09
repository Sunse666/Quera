using System.Text;

namespace Quera.Helpers;

internal static class Log
{
    private static readonly string Path = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "quera.log");

    public static void Info(string msg) => Write("INFO", msg);
    public static void Error(string msg, Exception? ex = null) => Write("ERROR", $"{msg} {ex}");

    private static void Write(string level, string msg)
    {
        try
        {
            var line = $"{DateTime.Now:HH:mm:ss.fff} [{level}] {msg}{Environment.NewLine}";
            File.AppendAllText(Path, line, Encoding.UTF8);
        }
        catch { /* can't log, ignore */ }
    }
}
