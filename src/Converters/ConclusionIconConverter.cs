using System.Globalization;
using System.Windows.Data;

namespace GitHubActionsVS.Converters;
public class ConclusionIconConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        string status = value as string;
        return GetConclusionIndicator(status);
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value;
    }

    private string GetConclusionIndicator(string status) => status.ToLowerInvariant() switch
    {
        "success" => "\uEBB3 ",
        "failure" => "\uEBB4 ",
        "cancelled" => "\uEABD ",
        "skipped" => "\uEABD ",
        _ => "🤷🏽",
    };
}
