using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace CRSim.ScreenSimulator.Converters
{
    public class EmptyToVisibilityConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is string str)
            {
                return string.IsNullOrWhiteSpace(str) ? Visibility.Collapsed : Visibility.Visible;
            }
            if (value is int i)
            {
                return i == 0 ? Visibility.Collapsed : Visibility.Visible;
            }
            return value is null ? Visibility.Collapsed : Visibility.Visible;
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
