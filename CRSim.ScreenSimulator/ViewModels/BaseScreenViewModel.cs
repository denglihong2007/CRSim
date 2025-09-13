using CommunityToolkit.Mvvm.ComponentModel;
using CRSim.Core.Abstractions;
using CRSim.Core.Models;
using CRSim.ScreenSimulator.Abstractions;
using CRSim.ScreenSimulator.Models;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;

namespace CRSim.ScreenSimulator.ViewModels
{
    public partial class BaseScreenViewModel : ObservableObject,IScreenViewModel
    {
        public ITimeService TimeService { get; set; }
        public readonly Settings _settings;
        public readonly TaskCompletionSource<bool> DataLoaded = new();
        [ObservableProperty]
        private DateTime _currentTime = new();
        [ObservableProperty]
        private string _text = "";
        [ObservableProperty]
        public int _location;
        [ObservableProperty]
        private Station _thisStation;
        [ObservableProperty]
        private string _thisPlatform;
        [ObservableProperty]
        private string _thisTicketCheck;
        [ObservableProperty]
        private Uri _video;
        public ObservableCollection<TrainInfo> ScreenA { get; private set; } = [];
        public ObservableCollection<TrainInfo> ScreenB { get; private set; } = []; 
        public System.Windows.Threading.Dispatcher UIDispatcher { get; set; }

        public List<TrainInfo> TrainInfo { get; set; } = [];

        public StationType StationType = StationType.Both;

        protected BaseScreenViewModel(ITimeService timeService, ISettingsService settingsService)
        {

            TimeService = timeService;
            _settings = settingsService.GetSettings();

            TimeService.OneSecondElapsed += OnTimeElapsed;

            TimeService.RefreshSecondsElapsed += RefreshData;
            TimeService.RefreshSecondsElapsed += RefreshDisplay;
            Initialize();
        }
        [ObservableProperty]
        private int _currentPageIndex = 0;
        public int PageCount
        {
            get
            {
                return Math.Min((int)Math.Ceiling((double)TrainInfo.Count / ItemsPerPage * ScreenCount.Value),_settings.MaxPages);
            }
        }
        public int ItemsPerPage = 1;

        //<summary>
        // 适用于翻页屏的屏幕个数参数，非翻页屏请不要设置。
        //</summary>
        public int? ScreenCount = null;

        private async void Initialize()
        {
            await DataLoaded.Task;
            RefreshDisplay(null, null);
        }
        public void RefreshData(object? sender, EventArgs e)
        {
            if (CurrentPageIndex == 0)
            {
                List<TrainInfo> itemsToRemove = [];
                foreach(TrainInfo trainInfo in TrainInfo)
                {
                    if (trainInfo.DepartureTime == null)
                    {
                        if (trainInfo.ArrivalTime.Value.Add(_settings.StopDisplayFromArrivalDuration) < TimeService.GetDateTimeNow())
                        {
                            itemsToRemove.Add(trainInfo);
                        }
                    }
                    else
                    {
                        if ((StationType == StationType.Arrival || StationType == StationType.Both ? trainInfo.DepartureTime.Value : trainInfo.DepartureTime.Value.Subtract(_settings.StopDisplayUntilDepartureDuration)) < TimeService.GetDateTimeNow())
                        {
                            itemsToRemove.Add(trainInfo);
                        }
                    }
                }
                foreach (var item in itemsToRemove)
                {
                    TrainInfo.Remove(item);
                }
            }
        }

        private void OnTimeElapsed(object? sender, EventArgs e)
        {
            CurrentTime = TimeService.GetDateTimeNow();
            OnPropertyChanged(nameof(ScreenA));
            OnPropertyChanged(nameof(ScreenB));
        }

        public void LoadData(Station station, TicketCheck? ticketCheck, string platform)
        {
            ThisStation = station;
            ThisPlatform = platform;
            ThisTicketCheck = ticketCheck?.Name;
            var trains = station.TrainStops;
            foreach (var trainNumber in trains)
            {

                if (trainNumber != null &&
                    (StationType == StationType.Both ||
                    trainNumber.StationType == StationType.Both ||
                    StationType == trainNumber.StationType) &&
                    (ticketCheck == null || trainNumber.TicketCheckIds.Contains(ticketCheck.Id)) &&
                    (platform == string.Empty || trainNumber.Platform == platform))
                {
                    var now = TimeService.GetDateTimeNow();
                    var today = now.Date;

                    if (_settings.LoadTodayOnly && today.Add((trainNumber.DepartureTime??trainNumber.ArrivalTime)!.Value) < now)
                    {
                        continue;
                    }

                    DateTime? AdjustTime(TimeSpan? time) =>
                        time.HasValue ? (today.Add(time.Value) > now ? today.Add(time.Value) : today.Add(time.Value).AddDays(1)) : null;

                    TrainInfo.Add(new TrainInfo
                    {
                        TrainNumber = trainNumber.Number,
                        Terminal = trainNumber.Terminal,
                        Origin = trainNumber.Origin,
                        ArrivalTime = AdjustTime(trainNumber.ArrivalTime),
                        DepartureTime = AdjustTime(trainNumber.DepartureTime),
                        TicketChecks = trainNumber.TicketCheckIds is null ? [] : [.. station.WaitingAreas
                            .SelectMany(w => w.TicketChecks)
                            .Where(tc => trainNumber.TicketCheckIds.Contains(tc.Id))
                            .Select(tc => tc.Name)],
                        WaitingArea = trainNumber.StationType == StationType.Arrival ? string.Empty : string.Join(" ", station.WaitingAreas.Where(x => x.TicketChecks.Any(y => trainNumber.TicketCheckIds.Contains(y.Id))).Select(x=>x.Name)),
                        Platform = trainNumber.Platform,
                        Length = trainNumber.Length,
                        Landmark = trainNumber.Landmark,
                        State = trainNumber.Status
                    });

                }
            }
            if(StationType == StationType.Arrival)
            {
                TrainInfo = [.. TrainInfo.OrderBy(x => x.ArrivalTime??x.DepartureTime)];
            }
            else
            {
                TrainInfo = [.. TrainInfo.OrderBy(x => x.DepartureTime??x.ArrivalTime)];
            }
            RefreshData(null, null);
            DataLoaded.SetResult(true);
        }
        public virtual void RefreshDisplay(object? sender, EventArgs e)
        {
            UIDispatcher.Invoke(() =>
            {
                if (ScreenCount == null)
                {
                    for (int i = 0; i < ItemsPerPage; i++)
                    {
                        ScreenA.Add(TrainInfo.Count > i ? TrainInfo[i] : new());
                    }
                    if (ScreenA.Count > ItemsPerPage)
                    {
                        for (int i = 0; i < ItemsPerPage; i++)
                        {
                            ScreenA.RemoveAt(0);
                        }
                    }
                    return;
                }
                int pageCount = (int)Math.Ceiling((double)TrainInfo.Count / (ItemsPerPage * ScreenCount.Value));
                int startIndex = CurrentPageIndex * ItemsPerPage * ScreenCount.Value;

                switch (ScreenCount)
                {
                    case 1:
                        {
                            ScreenA.Clear();
                            var items = TrainInfo.Skip(startIndex).Take(ItemsPerPage).ToList();
                            while (items.Count < ItemsPerPage)
                            {
                                items.Add(new TrainInfo());
                            }
                            foreach (var item in items)
                            {
                                ScreenA.Add(item);
                            }
                            break;
                        }
                    case 2:
                        {
                            ScreenA.Clear();
                            ScreenB.Clear();

                            var leftItems = TrainInfo.Skip(startIndex).Take(ItemsPerPage).ToList();
                            while (leftItems.Count < ItemsPerPage)
                            {
                                leftItems.Add(new TrainInfo());
                            }
                            foreach (var item in leftItems)
                            {
                                ScreenA.Add(item);
                            }

                            var rightItems = TrainInfo.Skip(startIndex + ItemsPerPage).Take(ItemsPerPage).ToList();
                            while (rightItems.Count < ItemsPerPage)
                            {
                                rightItems.Add(new TrainInfo());
                            }
                            foreach (var item in rightItems)
                            {
                                ScreenB.Add(item);
                            }
                            break;
                        }
                }

                CurrentPageIndex = CurrentPageIndex + 1 >= Math.Min(_settings.MaxPages, pageCount) ? 0 : CurrentPageIndex + 1;
            });
        }

    }
}
