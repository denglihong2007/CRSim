using Microsoft.UI.Xaml.Data;
using System.Globalization;

namespace CRSim.Converters
{
    public partial class TimeSpanToStringConverter : IValueConverter
    {

        public string Format { get; set; } = @"hh\:mm";
        public string Culture { get; set; } = "zh-CN";
        public string NullString { get; set; } = string.Empty;
        public string ZeroString { get; set; } = string.Empty;
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is TimeSpan timeSpan)
            {
                if (timeSpan == TimeSpan.Zero)
                {
                    return ZeroString;
                }
                string sign = timeSpan < TimeSpan.Zero ? "-" : string.Empty;
                TimeSpan absoluteValue = timeSpan.Duration();
                var cultureInfo = new CultureInfo(Culture);
                var timeOnly = absoluteValue - TimeSpan.FromDays(absoluteValue.Days);
                return sign + timeOnly.ToString(Format, cultureInfo);
            }

            if (value is null)
            {
                return NullString;
            }

            return string.Empty;
        }
        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
