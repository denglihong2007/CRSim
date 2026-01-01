namespace CRSim.ViewModels;

public partial class PlatformDiagramPageViewModel(IDialogService _dialogService, IDatabaseService _databaseService) : ObservableObject
{
    public string PageTitle = "生成站台占用图";
    private readonly ISettingsService _settingsService = App.GetService<ISettingsService>();

    public Station? SelectedStation;
    public List<Station> Stations => _databaseService.GetAllStations();

    [ObservableProperty]
    public partial bool Validated { get; set; } = false;

    [ObservableProperty]
    public partial int PageWidth { get; set; } = 5000;

    [RelayCommand]
    public void StationSelected(object s)
    {
        if(s is Station station)
        {
            SelectedStation = station;
        }
        Validate();
    }
    [RelayCommand]
    public void Validate()
    {
        Validated = SelectedStation != null && PageWidth > 0;
    }
    [RelayCommand]
    public async Task Generate()
    {
        if(!CheckStation(SelectedStation,out string detail))
        {
            await _dialogService.ShowTextAsync("警告", detail);
        }
        var path = _dialogService.SaveFile(".pdf", $"{SelectedStation.Name}站台占用图");
        if(string.IsNullOrEmpty(path) || SelectedStation == null) return;
        await Task.Run(() => Generator.Generate(SelectedStation, path,PageWidth, _settingsService.GetSettings().TrainColors));
    }

    public static bool CheckStation(Station station,out string detail)
    {
        detail = string.Empty;
        var trainStops = station.TrainStops;
        var platformGroups = trainStops
            .Where(ts => !string.IsNullOrEmpty(ts.Platform) && ts.DepartureTime != null && ts.ArrivalTime != null)
            .GroupBy(ts => ts.Platform);
        foreach (var group in platformGroups)
        {
            var stops = group.OrderBy(ts => ts.ArrivalTime ?? TimeSpan.Zero).ToList();
            for (int i = 0; i < stops.Count - 1; i++)
            {
                var current = stops[i];
                var next = stops[i + 1];
                // 判断时间是否重叠
                if (current.DepartureTime != null && next.ArrivalTime != null &&
                    current.DepartureTime > next.ArrivalTime)
                {
                    detail += $"\n站台 {group.Key} 车次 {current.Number} ({current.ArrivalTime:hh\\:mm}-{current.DepartureTime:hh\\:mm}) 与车次 {next.Number} ({next.ArrivalTime:hh\\:mm}-{next.DepartureTime:hh\\:mm}) 时间重叠；";
                }
            }
        }
        if(!string.IsNullOrEmpty(detail))
        {
            detail = $"检查到冲突 {detail.Count('\n')} 个！{detail}\n程序将会继续尝试绘制图像。";
            return false;
        }
        return true;
    }
}