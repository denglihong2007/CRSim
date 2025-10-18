namespace CRSim.ViewModels;
public partial class DashboardPageViewModel : ObservableObject
{
    [ObservableProperty]
    public partial string? UpdateMessage { get; set; } = null;
    public StyleManager StyleManager { get; }
    private readonly INetworkService _networkService;
    private readonly ISettingsService _settingsService;
    public DashboardPageViewModel(StyleManager styleManager,INetworkService networkService,ISettingsService settingsService)
    {
        StyleManager = styleManager;
        _networkService = networkService;
        _settingsService = settingsService;
        InitializeAsync();
    }
    private async void InitializeAsync()
    {
        var updateInfo = await _networkService.GetUpdateAsync(_settingsService.GetSettings().Api.UpdateApi);
        if (updateInfo is not null && updateInfo.Name != Assembly.GetExecutingAssembly().GetName().Version.ToString())
        {
            UpdateMessage = $"有新版本 {updateInfo.Name} 可用，请前往“设置”下载安装更新！";
        }
    }
}