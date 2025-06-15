using CRSim.Core.Models;
using CRSim.Core.Services;
using Microsoft.Extensions.DependencyInjection;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace CRSim.ScreenSimulator.Converters
{
    public class TrainStateConverter : IMultiValueConverter
    {
        private ITimeService _timeService;
        private Settings _settings;
        public string DisplayMode { get; set; } = "Normal";
        public string ArrivedText { get; set; } = "列车已到达";
        public string ArrivingText { get; set; } = "正点";
        public string WaitingText { get; set; } = "候车";
        public string CheckInText { get; set; } = "正在检票";
        public string StopCheckInText { get; set; } = "停止检票";
        object IMultiValueConverter.Convert(object[] values, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
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
            if (values[1] is DateTime departureTime && departureTime != new DateTime())
            {
                if (values[0] is DateTime)
                {
                    //过路站
                    if (_timeService.GetDateTimeNow() > departureTime.Subtract(_settings.PassingCheckInAdvanceDuration) && _timeService.GetDateTimeNow() < departureTime.Subtract(_settings.StopCheckInAdvanceDuration))
                    {
                        return CheckInText;
                    }
                }
                else
                {
                    //始发站
                    if (_timeService.GetDateTimeNow() > departureTime.Subtract(_settings.DepartureCheckInAdvanceDuration) && _timeService.GetDateTimeNow() < departureTime.Subtract(_settings.StopCheckInAdvanceDuration))
                    {
                        return CheckInText;
                    }
                }
                if (_timeService.GetDateTimeNow() > departureTime.Subtract(_settings.StopCheckInAdvanceDuration))
                {
                    return StopCheckInText;
                }
                if (values[2] is TimeSpan state && state.TotalMinutes > 0)
                {
                    return $"预计开点{departureTime.Add(state):HH:mm}";//晚点列车
                }
                return WaitingText;
            }
            return string.Empty;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            // 通常不需要实现 ConvertBack
            return null;
        }
    }
}