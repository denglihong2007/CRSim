using CRSim.Core.Abstractions;
using CRSim.ScreenSimulator.Models;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace CRSim.ScreenSimulator.Converters
{
    public class MetroDateTimeToStringConverter : IValueConverter, IHasTimeService
    {
        public ITimeService TimeService { get; set; }
        public string WaitingText { get; set; } = "请耐心等待";
        public string ComingText { get; set; } = "即将进站";
        public string ArrivedText { get; set; } = "列车进站";
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is TrainInfo trainInfo && parameter is string para)
            {
                if(trainInfo.TrainNumber == null)
                {
                    return WaitingText;
                }
                var timeDifference = (trainInfo.ArrivalTime ?? DateTime.MinValue) - TimeService.GetDateTimeNow();
                bool isArrivalTimeNull = trainInfo.ArrivalTime == null;
                var checkInAdvanceDuration = TimeSpan.FromMinutes(1);
                if (timeDifference < checkInAdvanceDuration)
                {
                    if (para == "TextBlock") 
                    {
                        if (!isArrivalTimeNull) 
                        {
                            if(trainInfo.ArrivalTime - TimeService.GetDateTimeNow() > TimeSpan.Zero)
                            {
                                return ComingText;
                            }
                        }
                        return ArrivedText;
                    }
                }
                else
                {
                    if (para == "StackPanel")
                        return Math.Round((trainInfo.ArrivalTime - TimeService.GetDateTimeNow()).Value.TotalMinutes).ToString();
                }
            }
            return string.Empty;
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
