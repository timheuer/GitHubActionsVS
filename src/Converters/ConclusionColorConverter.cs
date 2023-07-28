using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace GitHubActionsVS.Converters;

public class ConclusionColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        string status = value as string;
        return GetConclusionColor(status);
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value;
    }

    private SolidColorBrush GetConclusionColor(string status) => status.ToLowerInvariant() switch
    {
        "success" => new SolidColorBrush(Colors.Green),
        "failure" => new SolidColorBrush(Colors.Red),
        "startup_failure" => new SolidColorBrush(Colors.Red),
        "waiting" => new SolidColorBrush(Color.FromRgb(154, 103, 0)),
        _ => new SolidColorBrush(Colors.Black),
    };
}
