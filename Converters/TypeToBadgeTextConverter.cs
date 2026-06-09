namespace Quera.Converters;

public class TypeToBadgeTextConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, System.Globalization.CultureInfo culture)
    {
        if (value is SearchResultType type)
        {
            return type switch
            {
                SearchResultType.Command => "命令",
                SearchResultType.File => "文件",
                SearchResultType.App => "应用",
                SearchResultType.Folder => "文件夹",
                SearchResultType.Bookmark => "书签",
                SearchResultType.WebSearch => "搜索",
                SearchResultType.SearchHint => "搜索引擎",
                _ => type.ToString()
            };
        }
        return "";
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, System.Globalization.CultureInfo culture)
        => throw new NotSupportedException();
}
