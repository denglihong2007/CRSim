﻿using CRSim.Core.Converters;
using System.Text.Json.Serialization;

namespace CRSim.Core.Models
{
    public class Settings
    {
        public TimeSpan TimeOffset { get; set; } = TimeSpan.Zero;

        public TimeSpan DepartureCheckInAdvanceDuration { get; set; } = TimeSpan.FromMinutes(20);

        public TimeSpan PassingCheckInAdvanceDuration { get; set; } = TimeSpan.FromMinutes(10);

        /// <summary>
        /// 直到发车前多久停止显示
        /// </summary>
        public TimeSpan StopDisplayUntilDepartureDuration { get; set; } = TimeSpan.FromMinutes(1);

        /// <summary>
        /// 直到终到后多久停止显示
        /// </summary>
        public TimeSpan StopDisplayFromArrivalDuration { get; set; } = TimeSpan.FromMinutes(10);
        public TimeSpan StopCheckInAdvanceDuration { get; set; } = TimeSpan.FromMinutes(2);
        public string ApiUri { get; set; } = "http://47.122.74.193:25565";
        public int MaxPages { get; set; } = 3;
        public int SwitchPageSeconds { get; set; } = 20;
        public string UserKey { get; set; } = "";
        public bool LoadTodayOnly { get; set; } = false;
        public bool ReopenUnclosedScreensOnLoad { get; set; } = true;
    }
}
