using System.Globalization;
using System.Windows.Data;
using Binding = System.Windows.Data.Binding;

namespace MediaManager.UI.Converters;

public class FileSizeConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is long bytes)
            return bytes switch
            {
                >= 1_073_741_824 => $"{bytes / 1_073_741_824.0:F1} GB",
                >= 1_048_576     => $"{bytes / 1_048_576.0:F1} MB",
                >= 1_024         => $"{bytes / 1_024.0:F1} KB",
                _                => $"{bytes} B"
            };
        return value;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => Binding.DoNothing;
}
