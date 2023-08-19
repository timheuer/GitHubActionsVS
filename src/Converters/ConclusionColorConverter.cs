using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace GitHubActionsVS.Converters;

public class ConclusionColorConverter : IMultiValueConverter
{

    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        string status = values[0] as string;
        Brush defaultBrush = values[1] as Brush;

        return GetConclusionColor(status, defaultBrush);
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }

    private Brush GetConclusionColor(string status, Brush inheritedBrush) => status.ToLowerInvariant() switch
    {

        "success" => new SolidColorBrush(Colors.Green),
        "failure" => new SolidColorBrush(Colors.Red),
        "startup_failure" => new SolidColorBrush(Colors.Red),
        "waiting" => new SolidColorBrush(Color.FromRgb(154, 103, 0)),
        _ => inheritedBrush,
    };
}
