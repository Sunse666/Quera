using System.Globalization;
using System.Windows.Media.Imaging;

namespace Quera.Converters;

public class IconToImageConverter : IValueConverter
{
    private static readonly string[] ImageExts = { ".ico", ".png", ".jpg", ".jpeg", ".bmp", ".gif" };

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var icon = value as string;
        if (string.IsNullOrEmpty(icon)) return "\U0001F4C4";

        var ext = Path.GetExtension(icon).ToLowerInvariant();
        if (ImageExts.Contains(ext))
        {
            var fullPath = ExpandPath(icon);
            if (File.Exists(fullPath))
            {
                try
                {
                    var bmp = new BitmapImage();
                    bmp.BeginInit();
                    bmp.UriSource = new Uri(fullPath);
                    bmp.CacheOption = BitmapCacheOption.OnLoad;
                    bmp.EndInit();
                    bmp.Freeze();
                    return bmp;
                }
                catch { return icon; }
            }
        }
        return icon;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => throw new NotSupportedException();

    private static string ExpandPath(string path)
    {
        if (path.StartsWith("~"))
            return System.IO.Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                path[1..].TrimStart('\\', '/'));
        if (System.IO.Path.IsPathRooted(path)) return path;
        return System.IO.Path.GetFullPath(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, path));
    }
}
