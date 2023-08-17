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
        "completed" => "\uEBB3 ",
        "failure" => "\uEC13 ",
        "startup_failure" => "\uEC13 ",
        "cancelled" => "\uEC19 ",
        "skipped" => "\uEABD ",
        "pending" => "\uEB81 ",
        "queued" => "\uEBA7 ",
        "requested" => "\uEBA7 ",
        "waiting" => "\uEA82 ",
        "inprogress" => "\uEA82 ",
        "in_progress" => "\uEA82 ",
        "warning" => "\uEC1F ",
        null => "\uEA82 ",
        _ => "\uEA74 ",
    };
}
