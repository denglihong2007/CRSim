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
            if (value is TimeSpan dateTime)
            {
                if(dateTime == new TimeSpan())
                {
                    return ZeroString;
                }
                var CultureInfo = new CultureInfo(Culture);
                return (dateTime-dateTime.Days*TimeSpan.FromHours(24)).ToString(Format,CultureInfo);
            }
            if(value is null)
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
