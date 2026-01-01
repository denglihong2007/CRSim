namespace CRSim.Views.DialogContents;
public sealed partial class CreateStationDialog : Page
{

    private readonly IDialogService _dialogService = App.GetService<IDialogService>();

    private readonly IPluginService _pluginService = App.GetService<IPluginService>();

    private Station? _parsedStation = null;

    private record StationRecord(string Name, Station Station);

    private List<StationRecord> StationRecords { get; }

    public Station? GeneratedStation => ToBoolean(Plugin.IsChecked) ? (Station)comboBoxPlugin.SelectedValue :
        ToBoolean(Blank.IsChecked) ? new Station
    {
        Name = textBoxBlank.Text,
        Platforms = [],
        WaitingAreas = [],
        TrainStops = []
    } : _parsedStation;

    private readonly Action<bool> _onValidityChanged;

    public bool ToBoolean(bool? value)
    {
        return value == true;
    }

    private void Validate(object sender, object e)
    {
        if (textBoxBlank == null) return;//检查窗体初始化
        bool isValid;
        if (ToBoolean(Blank.IsChecked)) 
        {
            isValid = !string.IsNullOrWhiteSpace(textBoxBlank.Text);
        }
        else if(ToBoolean(Plugin.IsChecked))
        {
            isValid = comboBoxPlugin.SelectedValue is not null;
        }
        else
        {
            isValid = _parsedStation != null;
        }
        _onValidityChanged?.Invoke(isValid);
    }

    public CreateStationDialog(Action<bool> onValidityChanged)
    {
        StationRecords = _pluginService.GetStationData().Select(x => new StationRecord($"{x.Item1}-{x.Item2.Name}",x.Item2)).ToList();
        _onValidityChanged = onValidityChanged;
        InitializeComponent();
    }

    private async void ImportFromSS7DSAsync(object sender, RoutedEventArgs e)
    {
        var path = _dialogService.GetFile([".exe"]);
        textBoxBroadcast.Text = path;
        if (path == null) return;
        try
        {
            Station station = new() { Platforms = [], WaitingAreas = [], TrainStops = [] };
            var stationName = (await File.ReadAllTextAsync(path.Replace("车站广播系统.exe", "info.ini"))).Split("|")[0];
            var lines = await File.ReadAllLinesAsync(path.Replace("车站广播系统.exe", "data.csv"));
            station.Name = stationName;
            foreach (var line in lines)
            {
                var data = line.Split(',');
                var waitingAreaName = string.IsNullOrWhiteSpace(data.Last()) ? "未知" : data.Last();
                var platform = string.IsNullOrWhiteSpace(data[5]) ? "未知" : data[5];
                var ticketCheck = string.IsNullOrWhiteSpace(data[4]) ? string.Empty : data[4];

                var existingPlatform = station.Platforms.FirstOrDefault(x => x.Name == platform);
                if (existingPlatform == null)
                {
                    station.Platforms.Add(new Platform { Name = platform, Length = 20 });
                }


                var waitingArea = station.WaitingAreas.FirstOrDefault(x => x.Name == waitingAreaName);
                if (waitingArea == null)
                {
                    waitingArea = new WaitingArea() { Name = waitingAreaName };
                    station.WaitingAreas.Add(waitingArea);
                }

                foreach (var ticketCheckName in ticketCheck.Split('|'))
                {
                    if (!string.IsNullOrWhiteSpace(ticketCheckName) && !waitingArea.TicketChecks.Any(x => x.Name == ticketCheckName))
                    {
                        waitingArea.TicketChecks.Add(new TicketCheck { Name = ticketCheckName });
                    }
                }

                if (!string.IsNullOrEmpty(data[0]) && !station.TrainStops.Any(x => x.Number == data[0]))
                {
                    TimeSpan? arrivalTime = null;
                    TimeSpan? departureTime = null;

                    if (data[2] == stationName)
                    {
                        departureTime = TimeSpan.Parse(lines.FirstOrDefault(x => x.Split(",")[0] == data[0] && x.Split(",")[1].Contains("送车"))?.Split(",")[7] ?? string.Empty);
                    }
                    else if (data[3] == stationName)
                    {
                        arrivalTime = TimeSpan.Parse(data[7]);
                    }
                    else
                    {
                        arrivalTime = TimeSpan.Parse(data[7]);
                        departureTime = TimeSpan.Parse(lines.FirstOrDefault(x => x.Split(",")[0] == data[0] && x.Split(",")[1].Contains("送车"))?.Split(",")[7] ?? string.Empty);
                    }

                    station.TrainStops.Add(new TrainStop
                    {
                        Number = data[0],
                        Terminal = data[3],
                        Origin = data[2],
                        TicketCheckIds = [.. waitingArea.TicketChecks.Select(x => x.Id)],
                        ArrivalTime = arrivalTime,
                        DepartureTime = departureTime,
                        Platform = platform,
                        Length = data[0].StartsWith('G') || data[0].StartsWith('D') || data[0].StartsWith('C') ? Math.Abs(data[0].GetHashCode()) % 3 == 0 ? 8 : 16 : 18,
                        Landmark = data[8] + "色" ?? null,
                    });
                }
            }
            _parsedStation = station;
        }
        catch (Exception ex)
        {
            _parsedStation = null;
        }
        Validate(this, EventArgs.Empty);
    }

    private async void ImportFromFileAsync(object sender, RoutedEventArgs e)
    {
        var path = _dialogService.GetFile([".json"]);
        textBoxFile.Text = path;
        if (path == null) return;
        try
        {
            var json = File.ReadAllText(path).Replace("StationStop", "TrainStop");
            var data = JsonSerializer.Deserialize<Json>(json, JsonContext.Default.Json);
            _parsedStation = data?.Stations.FirstOrDefault();
        }
        catch (Exception ex)
        {
            _parsedStation = null;
        }
        Validate(this, EventArgs.Empty);
    }
}
