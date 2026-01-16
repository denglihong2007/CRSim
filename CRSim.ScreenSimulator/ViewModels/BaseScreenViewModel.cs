using CommunityToolkit.Mvvm.ComponentModel;
using CRSim.Core.Abstractions;
using CRSim.Core.Models;
using CRSim.ScreenSimulator.Abstractions;
using CRSim.ScreenSimulator.Models;
using System.Collections.ObjectModel;

namespace CRSim.ScreenSimulator.ViewModels
{
    public partial class BaseScreenViewModel : ObservableObject, IScreenViewModel
    {
        public ITimeService TimeService { get; set; }
        public readonly Settings _settings;
        public readonly TaskCompletionSource<bool> DataLoaded = new();

        [ObservableProperty] private DateTime _currentTime = new();
        [ObservableProperty] private string _text = "";
        [ObservableProperty] public int _location;
        [ObservableProperty] private Station _thisStation;
        [ObservableProperty] private string _thisPlatform;
        [ObservableProperty] private string _thisTicketCheck;
        [ObservableProperty] private Uri _video;
        [ObservableProperty] private int _currentPageIndex = 0;

        public ObservableCollection<TrainInfo> ScreenA { get; private set; } = [];
        public ObservableCollection<TrainInfo> ScreenB { get; private set; } = [];
        public System.Windows.Threading.Dispatcher UIDispatcher { get; set; }
        public List<TrainInfo> TrainInfo { get; set; } = [];
        public StationType StationType = StationType.Both;
        public int ItemsPerPage = 1;

        //<summary>
        // 适用于翻页屏的屏幕个数参数，非翻页屏请不要设置。
        //</summary>
        public int? ScreenCount = null;

        private TrainInfo nullTrainInfo = new();

        public int PageCount
        {
            get
            {
                if (ScreenCount == null || ScreenCount == 0) return 1;
                return Math.Min((int)Math.Ceiling((double)TrainInfo.Count / (ItemsPerPage * ScreenCount.Value)), _settings.MaxPages);
            }
        }

        protected BaseScreenViewModel(ITimeService timeService, ISettingsService settingsService)
        {
            TimeService = timeService;
            _settings = settingsService.GetSettings();

            // 每秒执行一次：包含数据清理和 UI 重建
            TimeService.OneSecondElapsed += OnTimeElapsed;

            // 仅执行翻页逻辑（例如每 10 秒切换一次页码）
            TimeService.RefreshSecondsElapsed += RefreshDisplay;

            Initialize();
        }

        private async void Initialize()
        {
            await DataLoaded.Task;
            UpdateVisuals(); // 初始渲染
        }

        /// <summary>
        /// 每秒触发的核心逻辑
        /// </summary>
        private void OnTimeElapsed(object? sender, EventArgs e)
        {
            CurrentTime = TimeService.GetDateTimeNow();

            // 1. 检查并删除过期车次
            CleanExpiredData();

            // 2. 重新构建显示内容（清空并重新添加）
            UpdateVisuals();
        }

        /// <summary>
        /// 仅负责翻页更改索引
        /// </summary>
        public virtual void RefreshDisplay(object? sender, EventArgs e)
        {
            int totalPages = PageCount;
            if (totalPages <= 1)
            {
                CurrentPageIndex = 0;
                return;
            }

            // 循环翻页逻辑
            CurrentPageIndex = (CurrentPageIndex + 1 >= totalPages) ? 0 : CurrentPageIndex + 1;
        }

        /// <summary>
        /// 核心渲染方法：删除所有元素并根据当前 PageIndex 重新填充
        /// </summary>
        private void UpdateVisuals()
        {
            UIDispatcher.Invoke(() =>
            {
                // 清空当前显示的所有元素
                ScreenA.Clear();
                ScreenB.Clear();

                if (ScreenCount == null)
                {
                    // 非翻页模式：始终显示前 N 条
                    for (int i = 0; i < ItemsPerPage; i++)
                    {
                        ScreenA.Add(TrainInfo.Count > i ? TrainInfo[i] : nullTrainInfo);
                    }
                    return;
                }

                // 翻页模式：根据 CurrentPageIndex 计算起始点
                int startIndex = CurrentPageIndex * ItemsPerPage * ScreenCount.Value;

                // 填充 ScreenA
                var leftItems = TrainInfo.Skip(startIndex).Take(ItemsPerPage).ToList();
                while (leftItems.Count < ItemsPerPage) leftItems.Add(nullTrainInfo);
                foreach (var item in leftItems) ScreenA.Add(item);

                // 如果是双屏模式，填充 ScreenB
                if (ScreenCount >= 2)
                {
                    var rightItems = TrainInfo.Skip(startIndex + ItemsPerPage).Take(ItemsPerPage).ToList();
                    while (rightItems.Count < ItemsPerPage) rightItems.Add(nullTrainInfo);
                    foreach (var item in rightItems) ScreenB.Add(item);
                }
            });
        }

        /// <summary>
        /// 数据清理逻辑：检查并移除不再需要显示的车次
        /// </summary>
        private void CleanExpiredData()
        {
            var now = TimeService.GetDateTimeNow();
            bool changed = false;

            // 使用倒序或创建临时列表移除，防止遍历冲突
            for (int i = TrainInfo.Count - 1; i >= 0; i--)
            {
                var train = TrainInfo[i];
                bool shouldRemove = false;

                if (train.DepartureTime == null) // 终到车
                {
                    if (train.ArrivalTime.Value.Add(_settings.StopDisplayFromArrivalDuration) < now)
                        shouldRemove = true;
                }
                else // 始发或过路车
                {
                    var referenceTime = (StationType == StationType.Arrival || StationType == StationType.Both)
                                        ? train.DepartureTime.Value
                                        : train.DepartureTime.Value.Subtract(_settings.StopDisplayUntilDepartureDuration);

                    if (referenceTime < now)
                        shouldRemove = true;
                }

                if (shouldRemove)
                {
                    TrainInfo.RemoveAt(i);
                    changed = true;
                }
            }

            // 如果删除了数据导致总页数变少，重置索引防止越界
            if (changed && CurrentPageIndex >= PageCount)
            {
                CurrentPageIndex = 0;
            }
        }

        /// <summary>
        /// 原始数据加载（保持不变）
        /// </summary>
        public void LoadData(Station station, TicketCheck? ticketCheck, string platform)
        {
            ThisStation = station;
            ThisPlatform = platform;
            ThisTicketCheck = ticketCheck?.Name;

            var trains = station.TrainStops;
            foreach (var trainNumber in trains)
            {
                if (trainNumber != null &&
                    (StationType == StationType.Both || trainNumber.StationType == StationType.Both || StationType == trainNumber.StationType) &&
                    (ticketCheck == null || trainNumber.TicketCheckIds.Contains(ticketCheck.Id)) &&
                    (platform == string.Empty || trainNumber.Platform == platform))
                {
                    var now = TimeService.GetDateTimeNow();
                    var today = now.Date;

                    if (_settings.LoadTodayOnly && today.Add((trainNumber.DepartureTime ?? trainNumber.ArrivalTime)!.Value).Add(trainNumber.Status.Value) < now)
                    {
                        continue;
                    }

                    DateTime? AdjustTime(TimeSpan? time, TimeSpan? status) =>
                        time.HasValue ? (today.Add(time.Value).Add(status.Value) > now ? today.Add(time.Value) : today.Add(time.Value).AddDays(1)) : null;

                    TrainInfo.Add(new TrainInfo
                    {
                        TrainNumber = trainNumber.Number,
                        Terminal = trainNumber.Terminal,
                        Origin = trainNumber.Origin,
                        ArrivalTime = AdjustTime(trainNumber.ArrivalTime, trainNumber.Status),
                        DepartureTime = trainNumber.Status > TimeSpan.Zero ? AdjustTime(trainNumber.DepartureTime, trainNumber.Status) : AdjustTime(trainNumber.DepartureTime, TimeSpan.Zero),
                        TicketChecks = trainNumber.TicketCheckIds is null ? [] : [.. station.WaitingAreas
                            .SelectMany(w => w.TicketChecks)
                            .Where(tc => trainNumber.TicketCheckIds.Contains(tc.Id))
                            .Select(tc => tc.Name)],
                        WaitingArea = trainNumber.StationType == StationType.Arrival ? string.Empty : string.Join(" ", station.WaitingAreas.Where(x => x.TicketChecks.Any(y => trainNumber.TicketCheckIds.Contains(y.Id))).Select(x => x.Name)),
                        Platform = trainNumber.Platform,
                        Length = trainNumber.Length,
                        Landmark = trainNumber.Landmark,
                        State = trainNumber.Status
                    });
                }
            }

            TrainInfo = StationType == StationType.Arrival
                ? [.. TrainInfo.OrderBy(x => x.ArrivalTime ?? x.DepartureTime)]
                : [.. TrainInfo.OrderBy(x => x.DepartureTime ?? x.ArrivalTime)];

            DataLoaded.SetResult(true);
        }
    }
}