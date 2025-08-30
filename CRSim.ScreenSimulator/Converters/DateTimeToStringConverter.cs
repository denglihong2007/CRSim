using System.Globalization;
using System.Windows.Data;

namespace CRSim.ScreenSimulator.Converters
{
    public class DateTimeToStringConverter : IValueConverter
    {

        public string Format { get; set; } = "yyyy-MM-dd HH:mm:ss";  // 默认格式是 "yyyy-MM-dd HH:mm:ss"
        public string Culture { get; set; } = "zh-CN";
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is DateTime dateTime && dateTime != new DateTime())
            {
                var CultureInfo = new CultureInfo(Culture);
                return dateTime.ToString(Format, CultureInfo);
            }
            return "--:--";
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // 通常不需要实现 ConvertBack
            return null;
        }
    }
}
