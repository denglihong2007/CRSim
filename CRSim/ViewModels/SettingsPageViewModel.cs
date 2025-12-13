using Downloader;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.IO.Packaging;
using System.Reflection;
using System.Security.Policy;
using Windows.System;

namespace CRSim.ViewModels
{
    public partial class SettingsPageViewModel : ObservableObject
    {
        [ObservableProperty]
        public partial string PageTitle { get; set; } = "设置";

        [ObservableProperty]
        public partial string AppVersion { get; set; } = "";
        public ObservableCollection<IApi> Apis { get; } =
        [
            new ApiFactory().CreateApi("官方站"),
            new ApiFactory().CreateApi("镜像站"),
        ];
        private Settings _settings;
        private readonly ISettingsService _settingsService;
        private readonly IDatabaseService _databaseService;
        private readonly INetworkService _networkService;
        private readonly IDialogService _dialogService;

        [ObservableProperty]
        public partial int UpdateProgress { get; set; }

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
        public partial IApi Api { get; set; }

        [ObservableProperty]
        public partial string UserKey { get; set; }

        [ObservableProperty]
        public partial bool LoadTodayOnly { get; set; }

        [ObservableProperty]
        public partial bool ReopenUnclosedScreensOnLoad { get; set; }
        #endregion
        public SettingsPageViewModel(ISettingsService settingsService, IDatabaseService databaseService, IDialogService dialogService, INetworkService networkService)
        {
            _settingsService = settingsService;
            _databaseService = databaseService;
            _dialogService = dialogService;
            _networkService = networkService;
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
            Api = Apis.First(x => x.Name == _settings.Api.Name);
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
            _settings.Api = Api;
            _settingsService.SaveSettings();
        }
        private static void UpdateSettings(string input, bool allowNegative, Action<int> updateAction)
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
        public async Task CheckUpdate()
        {
            var update = await _networkService.GetUpdateAsync(Api.UpdateApi);
            if (update is null)
            {
                await _dialogService.ShowMessageAsync("错误", "检查更新失败。");
            }
            else if (update.Name == AppVersion.Split(' ')[1])
            {
                await _dialogService.ShowMessageAsync("信息", "当前已经是最新版本。");
            }
            else
            {
                await _dialogService.ShowTextAsync("发现新版本 " + update.Name, update.Body + "\n请前往微软商店下载新版本。");
                await Launcher.LaunchUriAsync(new Uri("https://apps.microsoft.com/detail/9n4xhrrmph8v"));
            }
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