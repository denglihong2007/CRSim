using System.Diagnostics.Metrics;
using System.Reflection;

namespace CRSim.ViewModels
{
    public partial class SettingsPageViewModel : ObservableObject
    {
        [ObservableProperty]
        public partial string PageTitle { get; set; } = "设置";

        [ObservableProperty]
        public partial string AppVersion { get; set; } = "";
        public ObservableCollection<InfoItem> Apis { get; } =
        [
            new InfoItem { Title = "官方插件源", Detail = "http://47.122.74.193:25565" },
            new InfoItem { Title = "镜像站插件源", Detail = "https://crsim.com.cn/api" },
        ];
        private Settings _settings;
        private readonly ISettingsService _settingsService;
        private readonly IDatabaseService _databaseService;
        private readonly IDialogService _dialogService;
        #region 偏好设置
        [ObservableProperty]
        public partial string DepartureCheckInAdvanceDuration { get; set; }

        [ObservableProperty]
        public partial string PassingCheckInAdvanceDuration { get; set; }

        [ObservableProperty]
        public partial string StopDisplayUntilDepartureDuration { get; set; }

        [ObservableProperty]
        public partial string StopDisplayFromArrivalDuration { get; set; }

        [ObservableProperty]
        public partial string StopCheckInAdvanceDuration { get; set; }

        [ObservableProperty]
        public partial string MaxPages { get; set; }

        [ObservableProperty]
        public partial string SwitchPageSeconds { get; set; }

        [ObservableProperty]
        public partial InfoItem ApiUri { get; set; }

        [ObservableProperty]
        public partial string UserKey { get; set; }

        [ObservableProperty]
        public partial bool LoadTodayOnly { get; set; }

        [ObservableProperty]
        public partial bool ReopenUnclosedScreensOnLoad { get; set; }
        #endregion
        public SettingsPageViewModel(ISettingsService settingsService, IDatabaseService databaseService, IDialogService dialogService)
        {
            _settingsService = settingsService;
            _databaseService = databaseService;
            _dialogService = dialogService;
            var version = Assembly.GetExecutingAssembly().GetName().Version;
            AppVersion = $"Version {version}";
            LoadSettings();
        }

        private void LoadSettings()
        {
            _settings = _settingsService.GetSettings();
            DepartureCheckInAdvanceDuration = _settings.DepartureCheckInAdvanceDuration.TotalMinutes.ToString();
            PassingCheckInAdvanceDuration = _settings.PassingCheckInAdvanceDuration.TotalMinutes.ToString();
            StopDisplayUntilDepartureDuration = _settings.StopDisplayUntilDepartureDuration.TotalMinutes.ToString();
            StopDisplayFromArrivalDuration = _settings.StopDisplayFromArrivalDuration.TotalMinutes.ToString();
            StopCheckInAdvanceDuration = _settings.StopCheckInAdvanceDuration.TotalMinutes.ToString();
            ApiUri = Apis.Where(x => x.Detail == _settings.ApiUri).FirstOrDefault();
            MaxPages = _settings.MaxPages.ToString();
            SwitchPageSeconds = _settings.SwitchPageSeconds.ToString();
            UserKey = _settings.UserKey;
            LoadTodayOnly = _settings.LoadTodayOnly;
            ReopenUnclosedScreensOnLoad = _settings.ReopenUnclosedScreensOnLoad;
        }

        [RelayCommand]
        public void Unload()
        {
            UpdateSettings(DepartureCheckInAdvanceDuration, false, value => _settings.DepartureCheckInAdvanceDuration = TimeSpan.FromMinutes(value));
            UpdateSettings(PassingCheckInAdvanceDuration, false, value => _settings.PassingCheckInAdvanceDuration = TimeSpan.FromMinutes(value));
            UpdateSettings(StopDisplayUntilDepartureDuration, false, value => _settings.StopDisplayUntilDepartureDuration = TimeSpan.FromMinutes(value));
            UpdateSettings(StopDisplayFromArrivalDuration, false, value => _settings.StopDisplayFromArrivalDuration = TimeSpan.FromMinutes(value));
            UpdateSettings(StopCheckInAdvanceDuration, false, value => _settings.StopCheckInAdvanceDuration = TimeSpan.FromMinutes(value));
            UpdateSettings(MaxPages, false, value => _settings.MaxPages = value);
            UpdateSettings(SwitchPageSeconds, false, value => _settings.SwitchPageSeconds = value);
            _settings.UserKey = UserKey;
            _settings.LoadTodayOnly = LoadTodayOnly;
            _settings.ReopenUnclosedScreensOnLoad = ReopenUnclosedScreensOnLoad;
            _settings.ApiUri = ApiUri.Detail;
            _settingsService.SaveSettings();
        }
        private void UpdateSettings(string input, bool allowNegative, Action<int> updateAction)
        {
            if (int.TryParse(input, out int value) && (allowNegative || value >= 0))
            {
                updateAction(value);
            }
        }

        [RelayCommand]
        public async Task ClearData()
        {
            if (!await _dialogService.GetConfirmAsync("该操作将会清空所有数据，请否继续？")) return;
            _databaseService.ClearData();
            await _databaseService.SaveData();
        }
        [RelayCommand]
        public async Task ExportData()
        {
            string? path = _dialogService.SaveFile(".json", "data");
            if (string.IsNullOrWhiteSpace(path)) return;
            await _databaseService.ExportData(path);
        }
        [RelayCommand]
        public async Task ImportData()
        {
            string? path = _dialogService.GetFile([".json"]);
            if (string.IsNullOrWhiteSpace(path)) return;
            _databaseService.ImportData(path);
            await _databaseService.SaveData();
        }
    }
}