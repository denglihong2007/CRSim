using Microsoft.UI.Xaml.Data;

namespace CRSim.Converters
{
    public partial class IntToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return (value is int num && bool.TryParse((string)parameter,out bool target) && (target ? num != 0 : num==0))  ? Visibility.Collapsed : Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
