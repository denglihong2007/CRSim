using Microsoft.UI.Xaml.Data;
namespace CRSim.Converters
{
    public partial class DrawingToUIColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is System.Drawing.Color sColor)
            {
                return Windows.UI.Color.FromArgb(sColor.A, sColor.R, sColor.G, sColor.B);
            }
            return Windows.UI.Color.FromArgb(0,0,0,0);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            if (value is Windows.UI.Color wColor)
            {
                return System.Drawing.Color.FromArgb(wColor.A, wColor.R, wColor.G, wColor.B);
            }
            return System.Drawing.Color.FromArgb(0, 0, 0, 0);
        }
    }
}
