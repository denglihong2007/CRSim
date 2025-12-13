using CRSim.Core.Abstractions;
using CRSim.Core.Models;
using Microsoft.Extensions.DependencyInjection;
using System.Globalization;
using System.Windows.Data;

namespace CRSim.ScreenSimulator.Converters
{
    public class TrainStateConverter : IMultiValueConverter, IHasTimeService
    {
        public ITimeService TimeService { get; set; }
        private Settings _settings;
        public string DisplayMode { get; set; } = "Normal";
        public string ArrivedText { get; set; } = "列车已到达";
        public string ArrivingText { get; set; } = "正点";
        public string ArrivedLateText { get; set; } = "晚点{0}到达";
        public string ArrivingLateText { get; set; } = "预计晚点{0}";
        public string WaitingText { get; set; } = "候车";
        public string CheckInText { get; set; } = "正在检票";
        public string StopCheckInText { get; set; } = "停止检票";
        public string SuspendText { get; set; } = "列车停运";
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            _settings ??= StyleManager.ServiceProvider
                .GetRequiredService<ISettingsService>()
                .GetSettings();

            var now = TimeService.GetDateTimeNow();

            // 到达模式或关键数据缺失
            if (DisplayMode == "Arrive" || (values.Length > 1 && values[1] == null))
            {


                if (values[0] is DateTime arriveTime){
                    if (values.Length > 1)
                    {
                        if (values[2] is TimeSpan state)
                        {
                            // arriveTime += TimeSpan.FromMinutes(state.TotalMinutes);
                            if (now >= arriveTime)
                            {
                                if (state.TotalMinutes > 0)
                                    return string.Format(ArrivedLateText, ToHourMinuteString(state));
                                return ArrivedText;
                            }
                            else
                            {
                                if (state.TotalMinutes > 0)
                                    return string.Format(ArrivingLateText, ToHourMinuteString(state));
                                return ArrivingText;
                            }
                        }
                    }
                    return now >= arriveTime? ArrivedText : ArrivingText;
                }
                return string.Empty;
            }

            // 停运列车
            if (values[2] is null)
                return SuspendText;

            // 有出发时间
            if (values[1] is DateTime departureTime && departureTime != DateTime.MinValue)
            {
                bool isPassingStation = values[0] is DateTime;
                var checkInDuration = isPassingStation ? _settings.PassingCheckInAdvanceDuration : _settings.DepartureCheckInAdvanceDuration;

                // 检票中
                if (now > departureTime - checkInDuration && now < departureTime - _settings.StopCheckInAdvanceDuration)
                    return CheckInText;

                // 停止检票
                if (now >= departureTime - _settings.StopCheckInAdvanceDuration)
                    return StopCheckInText;

                // 晚点 / 正点
                if (values[2] is TimeSpan state)
                {
                    if (state.TotalMinutes > 0)
                        return $"晚点约{ToHourMinuteString(state)}"; // 晚点
                    return WaitingText; // 正点
                }
            }

            return string.Empty;
        }
        public static string ToHourMinuteString(TimeSpan ts)
        {
            int totalHours = (int)ts.TotalHours; // 可以超过24
            int minutes = ts.Minutes;

            if (totalHours > 0 && minutes > 0)
                return $"{totalHours}小时{minutes}分钟";
            if (totalHours > 0)
                return $"{totalHours}小时";
            if (minutes > 0)
                return $"{minutes}分钟";
            return "0分钟"; // 特殊情况，完全为0
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            // 通常不需要实现 ConvertBack
            return null;
        }
    }
}