using Microsoft.UI.Xaml.Data;

namespace CRSim.Converters
{
    public partial class NullToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            bool invert = (parameter as string)?.ToLower() == "invert";
            bool isNull = value == null;
            return invert ? isNull ? Visibility.Visible : Visibility.Collapsed : isNull ? Visibility.Collapsed : Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
