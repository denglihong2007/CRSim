using CRSim.Core.Abstractions;
using CRSim.ScreenSimulator.Models;
using Microsoft.Extensions.DependencyInjection;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace CRSim.ScreenSimulator.Converters
{
    public class MetroDateTimeToVisibilityConverter : IValueConverter, IHasTimeService
    {
        public ITimeService TimeService { get; set; }
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is TrainInfo trainInfo && parameter is string para)
            {
                var timeDifference = (trainInfo.ArrivalTime ?? DateTime.MinValue) - TimeService.GetDateTimeNow();
                var checkInAdvanceDuration = TimeSpan.FromMinutes(1);
                if (timeDifference < checkInAdvanceDuration)
                {
                    if (para == "TextBlock")
                        return Visibility.Visible;
                    else if (para == "StackPanel")
                        return Visibility.Collapsed;
                }
                else
                {
                    if (para == "TextBlock")
                        return Visibility.Collapsed;
                    else if (para == "StackPanel")
                        return Visibility.Visible;
                }
            }
            return Visibility.Collapsed;
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
