using CRSim.Core.Abstractions;
using CRSim.Core.Models;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace CRSim.ScreenSimulator.Converters
{
    public class TrainStateColorConverter : IMultiValueConverter, IHasTimeService
    {
        public ITimeService TimeService { get; set; }
        private Settings _settings;

        public string DisplayMode { get; set; } = "Normal";

        public List<SolidColorBrush> WaitingColorList { get; set; } = new();
        public SolidColorBrush ArrivedText { get; set; } = new(Colors.LightGreen);
        public SolidColorBrush ArrivingText { get; set; } = new(Colors.White);
        public SolidColorBrush ArrivingLateText { get; set; } = new(Colors.Red);
        public SolidColorBrush ArrivingEarlyText { get; set; } = new(Colors.LightGreen);
        public SolidColorBrush WaitingColor { get; set; } = new(Colors.White);
        public SolidColorBrush CheckInColor { get; set; } = new(Colors.LightGreen);
        public SolidColorBrush StopCheckInColor { get; set; } = new(Colors.Red);

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            _settings ??= StyleManager.ServiceProvider
                .GetRequiredService<ISettingsService>()
                .GetSettings();

            var now = TimeService.GetDateTimeNow();

            // 到达模式或缺少关键数据
            if (DisplayMode == "Arrive" || (values.Length > 1 && values[1] == null))
            {
                if (values[0] is DateTime arriveTime){
                    if (values.Length > 1 && values[1] is TimeSpan state){
                        if (state.TotalMinutes > 0) return ArrivingLateText;
                        if (state.TotalMinutes < 0) return ArrivingEarlyText;
                        return now >= arriveTime ? ArrivedText : ArrivingText;
                    }
                    return now >= arriveTime? ArrivedText : ArrivingText;
                }
                return new SolidColorBrush(Colors.Transparent);
            }

            // 列车停运
            if (values[2] is null)
                return StopCheckInColor;

            // 正点/晚点判断
            if (values[0] != null && values[1] == null && values[2] is TimeSpan status)
                return GetStatusColor(status);

            // 有出发时间
            if (values[1] is DateTime departureTime && departureTime != DateTime.MinValue)
            {
                bool isPassingStation = values[0] is DateTime;
                var checkInDuration = isPassingStation ? _settings.PassingCheckInAdvanceDuration : _settings.DepartureCheckInAdvanceDuration;

                // 候车中/检票中
                if (now > departureTime - checkInDuration && now < departureTime - _settings.StopCheckInAdvanceDuration)
                    return CheckInColor;

                // 停止检票
                if (now >= departureTime - _settings.StopCheckInAdvanceDuration)
                    return StopCheckInColor;

                // 正点/晚点
                if (values[2] is TimeSpan status2)
                    return GetStatusColor(status2, values.Length > 3 && values[3] is int row ? row : -1);
            }

            return new SolidColorBrush(Colors.Transparent);
        }

        private SolidColorBrush GetStatusColor(TimeSpan status, int rowNumber = -1)
        {
            if (status == TimeSpan.Zero)
            {
                if (DisplayMode == "Alternating_Row_Colors" && rowNumber >= 0 && WaitingColorList.Count > 0)
                    return WaitingColorList[rowNumber % WaitingColorList.Count];
                return WaitingColor; // 正点
            }
            else
            {
                return StopCheckInColor; // 晚点
            }
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
        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) => null;
    }
}
