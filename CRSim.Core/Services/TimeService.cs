using CRSim.Core.Abstractions;
using CRSim.Core.Models;
using System.Timers;

namespace CRSim.Core.Services
{
    public class TimeService : ITimeService
    {
        public DateTime SimulateTime { get; set; }

        private double _speed = 1.0;
        public double Speed
        {
            get => _speed;
            set
            {
                UpdateSimulatedOffset();  // 切换前先结算旧速率下的时间
                _speed = value;
                _lastSpeedChangeTime = DateTime.Now;
            }
        }

        private DateTime _lastSpeedChangeTime;     // 上一次倍速切换时的真实时间
        private TimeSpan _simulatedOffset = TimeSpan.Zero; // 累计模拟时间偏移（倍速放大之后的）

        private readonly System.Timers.Timer _oneSecondTimer;
        private readonly System.Timers.Timer _twentySecondsTimer;

        public event EventHandler OneSecondElapsed;
        public event EventHandler RefreshSecondsElapsed;

        private Settings _settings;

        public TimeService(ISettingsService settingsService)
        {
            _settings = settingsService.GetSettings();

            _oneSecondTimer = new(_settings.RefreshIntervalSeconds * 1000);
            _oneSecondTimer.Elapsed += OnOneSecondElapsed;

            _twentySecondsTimer = new(_settings.SwitchPageSeconds * 1000);
            _twentySecondsTimer.Elapsed += OnTwentySecondsElapsed;

            _lastSpeedChangeTime = DateTime.Now;
        }

        public void Start()
        {
            _oneSecondTimer.Start();
            _twentySecondsTimer.Start();
            _lastSpeedChangeTime = DateTime.Now;  // 启动时刷新时间
        }

        private void OnOneSecondElapsed(object sender, ElapsedEventArgs e)
        {
            OneSecondElapsed?.Invoke(this, EventArgs.Empty);
        }

        private void OnTwentySecondsElapsed(object sender, ElapsedEventArgs e)
        {
            RefreshSecondsElapsed?.Invoke(this, EventArgs.Empty);
        }

        public DateTime GetDateTimeNow()
        {
            var now = DateTime.Now;
            var elapsed = now - _lastSpeedChangeTime;
            return SimulateTime + _simulatedOffset + TimeSpan.FromTicks((long)(elapsed.Ticks * Speed));
        }

        /// <summary>
        /// 当倍速改变时，先结算之前段的累计偏移
        /// </summary>
        private void UpdateSimulatedOffset()
        {
            var now = DateTime.Now;
            var elapsed = now - _lastSpeedChangeTime;
            _simulatedOffset += TimeSpan.FromTicks((long)(elapsed.Ticks * Speed));
        }
    }
}