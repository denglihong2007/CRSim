using CRSim.Core.Abstractions;
using CRSim.Core.Converters;
using CRSim.Core.Models.PlatformDiagram;
using CRSim.Core.Services;
using System.Drawing;
using System.Text.Json.Serialization;

namespace CRSim.Core.Models
{
    public class Settings
    {
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
        [JsonIgnore]
        public IApi Api { get; set; } = new ApiFactory().CreateApi("镜像站");
        public string ApiName { get; set; } = "镜像站";
        public int MaxPages { get; set; } = 3;
        public int SwitchPageSeconds { get; set; } = 20;
        public string UserKey { get; set; } = "";
        public bool LoadTodayOnly { get; set; } = false;
        public bool ReopenUnclosedScreensOnLoad { get; set; } = true;

        public List<TrainColor> TrainColors { get; set; } =
        [
            new TrainColor { Prefix = "G", Color = Color.Magenta },
            new TrainColor { Prefix = "D", Color = Color.SteelBlue },
            new TrainColor { Prefix = "C", Color = Color.Teal },
            new TrainColor { Prefix = "T", Color = Color.Blue },
            new TrainColor { Prefix = "Z", Color = Color.SaddleBrown },
            new TrainColor { Prefix = "K", Color = Color.Red },
            new TrainColor { Prefix = "Y", Color = Color.Red },
            new TrainColor { Prefix = "L", Color = Color.Green },
            new TrainColor { Prefix = "默认", Color = Color.Green }
        ];
    }
}
