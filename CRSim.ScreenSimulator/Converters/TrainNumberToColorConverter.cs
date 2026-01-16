using CRSim.Core.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace CRSim.ScreenSimulator.Converters
{
    public class TrainNumberToColorConverter : IValueConverter
    {
        object IValueConverter.Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var _settings = StyleManager.ServiceProvider
                .GetRequiredService<ISettingsService>()
                .GetSettings();
            var trainColors = _settings.TrainColors;
            if (value is string trainNumber)
            {
                var matchedColor = trainColors
                    .Where(tc => trainNumber.StartsWith(tc.Prefix))
                    .OrderByDescending(tc => tc.Prefix.Length)
                    .FirstOrDefault();
                var color = matchedColor?.Color ?? trainColors.FirstOrDefault(x => x.Prefix == "默认").Color;
                var mediaColor = System.Windows.Media.Color.FromArgb(color.A, color.R, color.G, color.B);
                return new SolidColorBrush(mediaColor);
            }
            return new SolidColorBrush(Color.FromRgb(0, 0, 0));
        }

        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
