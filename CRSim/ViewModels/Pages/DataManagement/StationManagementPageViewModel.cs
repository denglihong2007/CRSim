namespace CRSim.ViewModels;

public partial class StationManagementPageViewModel : ObservableObject 
{
    public class TicketCheck
    {
        public string WaitingAreaName { get; set; }
        public string Name { get; set; }
    }
    [ObservableProperty]
	private string _pageTitle = "��վ����";
    
	[ObservableProperty]
	private string _pageDescription = "���������Ǻ���ܰ�����ѣ�����ǵñ���! Owo";

    [ObservableProperty]
    private bool _isSelected = false;

    [ObservableProperty]
    private List<string> _stationNames = [];

    [ObservableProperty]
    private Station _selectedStation = new();

    public ObservableCollection<string> WaitingAreaNames { get; private set; } = [];
    public ObservableCollection<Platform> Platforms { get; private set; } = [];

    public ObservableCollection<TicketCheck> TicketChecks { get; private set; } = [];

    public ObservableCollection<StationStop> StationStops { get; private set; } = [];

    private readonly IDatabaseService _databaseService;

    private readonly IDialogService _dialogService;

    private readonly INetworkService _networkService;
    public StationManagementPageViewModel(IDatabaseService databaseService,IDialogService dialogService,INetworkService networkService)
    {
		_databaseService = databaseService;
        _dialogService = dialogService;
        _networkService = networkService;
        RefreshStationList();
    }

	public void RefreshStationList()
    {
        var stationsList = _databaseService.GetAllStations();
        StationNames = [.. stationsList.Select(s => s.Name)];
    }

    [RelayCommand]
    public void StationSelected(object selectedStation)
    {
        WaitingAreaNames.Clear();
        Platforms.Clear();
        TicketChecks.Clear();
        StationStops.Clear();
        if(selectedStation is string selectedStationName)
        {
            SelectedStation = _databaseService.GetStationByName(selectedStationName);
            foreach(WaitingArea waitingArea in SelectedStation.WaitingAreas)
            {
                WaitingAreaNames.Add(waitingArea.Name);
                foreach (string ticketCheck in waitingArea.TicketChecks)
                {
                    TicketChecks.Add(new TicketCheck { WaitingAreaName = waitingArea.Name,Name=ticketCheck });
                }
            }
            foreach (StationStop stationStop in SelectedStation.StationStops)
            {
                StationStops.Add(stationStop);
            }
            foreach(var platform in SelectedStation.Platforms)
            {
                Platforms.Add(platform);
            }
            IsSelected = true;
        }
        if(selectedStation == null)
        {
            IsSelected = false;
        }
    }

    [RelayCommand]
    public void AddTicketCheck()
    {
        (string waitingAreaName,List<string> ticketChecks) = _dialogService.GetInputTicketCheck([.. WaitingAreaNames]);
        if (ticketChecks != null && waitingAreaName != null)
        {
            foreach(string ticketCheck in ticketChecks)
            {
                if (TicketChecks.Select(x => x.Name).Contains(ticketCheck))
                {
                    _dialogService.ShowMessage("���ʧ��", $"��� {ticketCheck} ��Ʊ���Ѵ��ڡ�");
                    return;
                }
                TicketChecks.Add(new TicketCheck { WaitingAreaName = waitingAreaName, Name = ticketCheck });
            }
        }
    }

    [RelayCommand]
    public void DeleteTicketCheck(object selectedTicketCheck)
    {
        if(selectedTicketCheck is TicketCheck ticketCheck)
        {
            TicketChecks.Remove(ticketCheck);
        }
    }
    [RelayCommand]
    public void DeleteAllTicketCheck()
    {
        if (!_dialogService.GetConfirm("��ǰ������ɾ��ȫ����Ʊ�ڣ��Ƿ������")) return;
        TicketChecks.Clear();
    }
    [RelayCommand]
    public void AddWaitingArea()
    {
        string? waitingAreaName = _dialogService.GetInput("�������������");
        if (waitingAreaName != null)
        {
            if (WaitingAreaNames.Contains(waitingAreaName))
            {
                _dialogService.ShowMessage("���ʧ��",$"���� {waitingAreaName} �����Ѵ��ڡ�");
                return;
            }
            WaitingAreaNames.Add(waitingAreaName);
        }
    }

    [RelayCommand]
    public void DeleteWaitingArea(object selectedWaitingArea)
    {
        if(selectedWaitingArea is string n)
        {
            WaitingAreaNames.Remove(n);
        }
    }
    [RelayCommand]
    public void DeleteAllWaitingArea()
    {
        if (!_dialogService.GetConfirm("��ǰ������ɾ��ȫ�����ң��Ƿ������")) return;
        WaitingAreaNames.Clear();
    }
    [RelayCommand]
    public void AddPlatform()
    {
        var platforms = _dialogService.GetInputPlatform();
        if (platforms != null)
        {
            foreach (var platform in platforms)
            {
                if (Platforms.Any(x=>x.Name==platform.Name))
                {
                    _dialogService.ShowMessage("���ʧ��", $"��� {platform.Name} վ̨�Ѵ��ڡ�");
                    return;
                }
                Platforms.Add(platform);
            }
        }
    }

    [RelayCommand]
    public void DeletePlatform(object selectedPlatform)
    {
        if (selectedPlatform is Platform p)
        {
            Platforms.Remove(p);
        }
    }
    [RelayCommand]
    public void DeleteAllPlatform()
    {
        if (!_dialogService.GetConfirm("��ǰ������ɾ��ȫ��վ̨���Ƿ������")) return;
        Platforms.Clear();
    }

    [RelayCommand]
    public async Task AddStation()
    {
        string? station = _dialogService.GetInput("�����복վ����");
        if (station != null)
        {
            if (StationNames.Contains(station))
            {
                _dialogService.ShowMessage("���ʧ��", $"��վ {station} �Ѵ��ڡ�");
                return;
            }
            _databaseService.AddStationByName(station);
            await _databaseService.SaveData();
            RefreshStationList();
        }
    }

    [RelayCommand]
    public async Task DeleteStation(object selectedStation)
    {
        if (selectedStation is string s && !string.IsNullOrEmpty(s))
        {
            _databaseService.DeleteStation(_databaseService.GetStationByName(s));
            await _databaseService.SaveData();
            RefreshStationList();
        }
    }
    [RelayCommand]
    public async Task DeleteAllStations()
    {
        if (!_dialogService.GetConfirm("��ǰ������ɾ��ȫ����վ���Ƿ������")) return;
        foreach(string stationName in StationNames)
        {
            _databaseService.DeleteStation(_databaseService.GetStationByName(stationName));
        }
        await _databaseService.SaveData();
        RefreshStationList();
    }
    [RelayCommand]
    public async Task SaveChanges()
    {
        if (!(await Validate())) return;
        _databaseService.UpdateStation(SelectedStation.Name, GenerateStation(SelectedStation.Name,WaitingAreaNames,TicketChecks,StationStops,Platforms));
        await _databaseService.SaveData();
    }

    private Task<bool> Validate()
    {
        string message = "";
        foreach(var t in TicketChecks)
        {
            if (!WaitingAreaNames.Contains(t.WaitingAreaName))
            {
                message += $"\n��Ʊ�� {t.Name} ���ڵĺ��� {t.WaitingAreaName} �����ڣ�";
            }
        }
        foreach(var s in StationStops)
        {
            if(!Platforms.Any(x=> x.Name == s.Platform))
            {
                message += $"\n���� {s.Number} �������վ̨ {s.Platform} �����ڣ�";
            }
            foreach(var t in s.TicketChecks)
            {
                if (!TicketChecks.Any(x => x.Name==t))
                {
                    message += $"\n���� {s.Number} ������ļ�Ʊ�� {t} �����ڣ�";
                }
            }
        }
        if (message == "") return Task.FromResult(true);
        _dialogService.ShowMessage("����ʧ��", $"���� {message.Split("\n").Length-1} ������{message}\n���޸����д�����ٴγ��Ա��档");
        return Task.FromResult(false);
    }

    public static Station GenerateStation(string stationName, ObservableCollection<string> waitingAreaNames, ObservableCollection<TicketCheck> ticketChecks,ObservableCollection<StationStop> stationStops,ObservableCollection<Platform> platforms)
    {
        var station = new Station
        {
            Name = stationName,
            WaitingAreas = [.. waitingAreaNames.Select(name => new WaitingArea { Name = name })],
            StationStops = [.. stationStops],
            Platforms = [..platforms]
        };

        // �� TicketCheck ӳ�䵽��Ӧ�� WaitingArea
        foreach (var ticketCheck in ticketChecks)
        {
            var waitingArea = station.WaitingAreas.FirstOrDefault(wa => wa.Name == ticketCheck.WaitingAreaName);
            waitingArea?.TicketChecks.Add(ticketCheck.Name);
        }

        return station;
    }
    [RelayCommand]
    public async Task ImportFromTimeTable()
    {
        List<string> stationNamesToAdd = [];
        foreach(TrainNumber trainNumber in _databaseService.GetAllTrainNumbers())
        {
            var stationNames = trainNumber.TimeTable.Select(x => x.Station).ToList();
            foreach(string stationName in stationNames)
            {
                if(!stationNamesToAdd.Contains(stationName) && !StationNames.Contains(stationName))
                {
                    stationNamesToAdd.Add(stationName);
                }
            }
        }
        foreach(string stationName in stationNamesToAdd)
        {
            _databaseService.AddStationByName(stationName);
        }
        await SaveChanges();
        RefreshStationList();
    }
    [RelayCommand]
    public async Task ImportFromLulutong()
    {
        if (TicketChecks.Count == 0)
        {
            _dialogService.ShowMessage("����ʧ��", "������Ӽ�Ʊ�ڡ�");
            return;
        }
        if (Platforms.Count == 0)
        {
            _dialogService.ShowMessage("����ʧ��", "�������վ̨��");
            return;
        }
        var path = _dialogService.GetFile("CSV �ļ� (*.csv)|*.csv|�����ļ� (*.*)|*.*");
        if (path == null) return;
        try
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            var lines = new List<string>();
            var reader = new StreamReader(path, Encoding.GetEncoding("GBK"));
            while (!reader.EndOfStream)
            {
                var line = await reader.ReadLineAsync();
                lines.Add(line);
            }
            foreach (var line in lines.Skip(13))
            {
                var data = line.Split(",");
                var firstColumn = data[0].Trim();
                if (!string.IsNullOrEmpty(firstColumn) && char.IsLetterOrDigit(firstColumn[0]))
                {
                    int spaceIndex = firstColumn.IndexOf(' ');
                    if (spaceIndex != -1)
                    {
                        firstColumn = firstColumn[..spaceIndex];
                    }
                    if (StationStops.Any(x=> x.Number==firstColumn)) continue;
                    DateTime? departureTime = DateTime.Parse(data[4]);
                    DateTime? arrivalTime = departureTime.Value.Subtract(TimeSpan.FromMinutes(2));
                    if (data[1].Split("-")[0] == SelectedStation.Name)
                    {
                        arrivalTime = null;
                    }
                    else if (data[1].Split("-")[1] == SelectedStation.Name)
                    {
                        departureTime = null;
                    }
                    StationStops.Add(new StationStop()
                    { 
                        Number = firstColumn, 
                        Origin = data[1].Split("-")[0], 
                        Terminal = data[1].Split("-")[1], 
                        ArrivalTime = arrivalTime,
                        DepartureTime = departureTime, 
                        TicketChecks = [TicketChecks[new Random().Next(TicketChecks.Count)].Name],
                        Platform = Platforms[new Random().Next(Platforms.Count)].Name,
                        Length = firstColumn.StartsWith('G') || firstColumn.StartsWith('D') || firstColumn.StartsWith('C') ? Math.Abs(firstColumn.GetHashCode()) % 3 == 0 ? 8 : 16 : 18,
                        Landmark = new[] { "��ɫ", "��ɫ", "��ɫ", "��ɫ", "��ɫ", "��ɫ", "��ɫ",null}[new Random().Next(8)]
                    });
                }
            }
        }
        catch
        {
            _dialogService.ShowMessage("����ʧ��", "�ļ���ʽ�����ռ�á�");
        }
    }
    [RelayCommand]
    public async Task ImportFrom7D()
    {
        var path = _dialogService.GetFile("��ִ�г����ļ� (*.exe)|*.exe|�����ļ� (*.*)|*.*");
        if (path == null) return;
        try
        {
            await _databaseService.ImportStationFrom7D(path);
            await _databaseService.SaveData();
            RefreshStationList();
        }
        catch
        {
            _dialogService.ShowMessage("����ʧ��", "�ļ���ʽ�����ռ�á�");
        }
    }
    [RelayCommand]
    public void AddStationStop()
    {
        var newStationStop = _dialogService.GetInputStationStop([.. TicketChecks.Select(x => x.Name)], [.. Platforms.Select(x => x.Name)]);
        if (newStationStop != null)
        {
            if (StationStops.Any(x => x.Number == newStationStop.Number))
            {
                _dialogService.ShowMessage("���ʧ��", $"���� {newStationStop.Number} �Ѵ��ڡ�");
                return;
            }
            StationStops.Add(newStationStop);
        }
    }
    [RelayCommand]
    public void DeleteStationStop(object selectedStationStop)
    {
        if (selectedStationStop is StationStop s)
        {
            StationStops.Remove(s);
        }
    }
    [RelayCommand]
    public void DeleteAllStationStop()
    {
        if (!_dialogService.GetConfirm("��ǰ������ɾ��ȫ�����Σ��Ƿ������")) return;
        StationStops.Clear();
    }
    [RelayCommand]
    public void EditStationStop(object _selectedStationStop)
    {
        if (_selectedStationStop is StationStop selectedStationStop)
        {
            var newStationStop = _dialogService.GetInputStationStop([.. TicketChecks.Select(x => x.Name)], [.. Platforms.Select(x => x.Name)], selectedStationStop);
            if (newStationStop != null)
            {
                StationStops[StationStops.IndexOf(selectedStationStop)] = newStationStop;
            }
        }
    }
    [RelayCommand]
    public async Task ImportFromInternet()
    {
        if (TicketChecks.Count == 0)
        {
            _dialogService.ShowMessage("����ʧ��", "������Ӽ�Ʊ�ڡ�");
            return;
        }
        if (Platforms.Count == 0)
        {
            _dialogService.ShowMessage("����ʧ��", "�������վ̨��");
            return;
        }
        if (StationStops.Count != 0)
        {
            if (!_dialogService.GetConfirm("��ǰ���������ʱ�̱��Ƿ������"))
            {
                return;
            }
        }

        var stops = await _networkService.GetStationStopsAsync(SelectedStation.Name);
        if (stops.Count !=0)
        {
            StationStops.Clear();
            foreach (StationStop t in stops)
            {
                t.TicketChecks = [TicketChecks[new Random().Next(TicketChecks.Count)].Name];
                t.Platform = Platforms[new Random().Next(Platforms.Count)].Name;
                t.Landmark = new[] { "��ɫ", "��ɫ", "��ɫ", "��ɫ", "��ɫ", "��ɫ", "��ɫ", null }[new Random().Next(8)];
                t.Length = t.Number.StartsWith('G') || t.Number.StartsWith('D') || t.Number.StartsWith('C') ? Math.Abs(t.Number.GetHashCode()) % 3 == 0 ? 8 : 16 : 18;
                StationStops.Add(t);
            }
        }
        else
        {
            _dialogService.ShowMessage("��ȡʧ��", "��������æ��վ�����ڡ�");
        }
    }

}
