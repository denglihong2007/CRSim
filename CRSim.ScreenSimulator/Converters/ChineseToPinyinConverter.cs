using System.Globalization;
using System.Windows.Data;

namespace CRSim.ScreenSimulator.Converters
{
    public class ChineseToPinyinConverter : IValueConverter
    {
        public bool Upper { get; set; } = false;
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not string chineseText) return string.Empty;
            if (string.IsNullOrWhiteSpace(chineseText))
                return string.Empty;
            var result = Core.Converters.ChineseToPinyinConverter.ConvertToPinyin(chineseText.Replace(" ", ""));
            return Upper ? result.ToUpper() : char.ToUpper(result[0], culture) + result[1..];
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
