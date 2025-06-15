using CRSim.Core.Models;
using CRSim.Core.Services;
using Microsoft.Extensions.DependencyInjection;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
namespace CRSim.ScreenSimulator.Converters
{
    public class TrainStateColorConverter : IMultiValueConverter
    {
        private ITimeService _timeService;
        private Settings _settings;
        public string DisplayMode { get; set; } = "Normal";

        /*
        Normal: 标准显示。
        Alternating_Row_Colors: 候车状态隔行异色显示（第4个参数控制行号）。
        Arrive: 到达屏显示
        */

        public List<SolidColorBrush> WaitingColorList { get; set; } = [];
        public SolidColorBrush ArrivedText { get; set; } = new(Colors.LightGreen);
        public SolidColorBrush ArrivingText { get; set; } = new(Colors.White);
        public SolidColorBrush WaitingColor { get; set; } = new(Colors.White);
        public SolidColorBrush CheckInColor { get; set; } = new(Colors.LightGreen);
        public SolidColorBrush StopCheckInColor { get; set; } = new(Colors.Red);
        object IMultiValueConverter.Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            var serviceProvider = (IServiceProvider)Application.Current.Resources["ServiceProvider"];
            _timeService = serviceProvider.GetRequiredService<ITimeService>();
            _settings = serviceProvider.GetRequiredService<ISettingsService>().GetSettings();
            if (DisplayMode == "Arrive" || ( values.Length > 1 && values[1] == null))
            {
                if (values[0] is DateTime arriveTime)
                {
                    return _timeService.GetDateTimeNow() >= arriveTime ? ArrivedText : ArrivingText;
                }
                return string.Empty;
            }
            if (values[0] != null && values[1] == null)
            {
                return WaitingColor;
            }
            if (values[1] is DateTime departureTime && departureTime !=new DateTime())
            {
                if (values[0] is DateTime)
                {
                    //过路站
                    if (_timeService.GetDateTimeNow() > departureTime.Subtract(_settings.PassingCheckInAdvanceDuration) && _timeService.GetDateTimeNow() < departureTime.Subtract(_settings.StopCheckInAdvanceDuration))
                    {
                        return CheckInColor;
                    }
                }
                else
                {
                    //始发站
                    if (_timeService.GetDateTimeNow() > departureTime.Subtract(_settings.DepartureCheckInAdvanceDuration) && _timeService.GetDateTimeNow() < departureTime.Subtract(_settings.StopCheckInAdvanceDuration))
                    {
                        return CheckInColor;
                    }
                }
                if (_timeService.GetDateTimeNow() > departureTime.Subtract(_settings.StopCheckInAdvanceDuration))
                {
                    return StopCheckInColor;
                }
                if (values[2] is TimeSpan state && state.TotalMinutes > 0)
                {
                    return StopCheckInColor;
                }
                if (DisplayMode == "Alternating_Row_Colors" && values.Length > 3 && values[3] is int rowNumber){
                    return WaitingColorList[rowNumber % WaitingColorList.Count];
                }
                return WaitingColor;
            }
            return new SolidColorBrush(Colors.Transparent);
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            // 通常不需要实现 ConvertBack
            return null;
        }
    }
}
