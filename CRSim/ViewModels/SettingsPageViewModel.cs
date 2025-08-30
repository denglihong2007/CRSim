using Downloader;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.IO.Packaging;
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
            new InfoItem { Title = "官方源", Detail = "http://47.122.74.193:25565" },
            new InfoItem { Title = "镜像站源", Detail = "https://crsim.com.cn/api" },
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
        public partial InfoItem ApiUri { get; set; }

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
            var update = ApiUri.Title == "官方源" ? 
                await _networkService.GetUpdateAsync("https://api.github.com/repos/denglihong2007/CRSim/releases/latest") : 
                await _networkService.GetUpdateAsync("https://crsim.com.cn/api/version");
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
                await _dialogService.ShowTextAsync("发现新版本 " + update.Name, update.Body + "\n系统即将下载安装新版本。");
                DispatcherQueue dispatcherQueue = DispatcherQueue.GetForCurrentThread();
                var downloader = new DownloadService();
                UpdateProgress = 0;
                try
                {
                    int lastProgress = 0;
                    var lastUpdate = DateTime.MinValue;
                    downloader.DownloadProgressChanged += (s, e) =>
                    {
                        var now = DateTime.Now;
                        if ((e.ProgressPercentage - lastProgress >= 1) || (now - lastUpdate).TotalMilliseconds >= 100)
                        {
                            lastProgress = (int)e.ProgressPercentage;
                            lastUpdate = now;

                            dispatcherQueue.TryEnqueue(() =>
                            {
                                UpdateProgress = (int)e.ProgressPercentage;
                            });
                        }
                    };
                    var path = Path.Combine(AppPaths.TempPath, "update.zip");
                    await downloader.DownloadFileTaskAsync(update.Assets[0].BrowserDownloadUrl, path);
                    UpdateProgress = 0;

                    if (!FileHashHelper.VerifySHA256(path, update.Assets[0].Digest.Split(':')[1]))
                    {
                        await _dialogService.ShowMessageAsync("错误", "下载的文件校验失败，请重试。");
                        return;
                    }

                    var programDirectory = AppDomain.CurrentDomain.BaseDirectory;
                    var appExePath = Process.GetCurrentProcess().MainModule.FileName;
                    var appName = Path.GetFileName(appExePath);
                    string batchScript = $@"
                        @echo off
                        taskkill /F /IM ""{appName}"" >nul 2>&1
                        timeout /t 2 >nul
                        cd /d ""{programDirectory}""
                        powershell -Command ""Expand-Archive -Path '{path}' -DestinationPath '{programDirectory}' -Force""
                        del /f /q ""{path}""
                        start """" ""{appExePath}""
                        exit /b 0
                        ";
                    string batchFilePath = Path.Combine(Path.GetTempPath(), "CRSimUpdate.bat");
                    File.WriteAllText(batchFilePath, batchScript);
                    var processInfo = new ProcessStartInfo
                    {
                        FileName = "cmd.exe",
                        Arguments = $"/c \"{batchFilePath}\"",
                        WindowStyle = ProcessWindowStyle.Hidden,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    };

                    Process.Start(processInfo);
                    Application.Current.Exit();
                }
                catch (Exception e)
                {
                    await _dialogService.ShowTextAsync("错误", "更新失败。\n" + e);
                    UpdateProgress = 0;
                    return;
                }
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