using CRSim.Core.Models;
using OfficeOpenXml;

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

    public ObservableCollection<TrainStop> TrainStops { get; private set; } = [];

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
        TrainStops.Clear();
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
            foreach (TrainStop trainStop in SelectedStation.TrainStops)
            {
                TrainStops.Add(trainStop);
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
        _databaseService.UpdateStation(SelectedStation.Name, GenerateStation(SelectedStation.Name,WaitingAreaNames,TicketChecks,TrainStops,Platforms));
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
        foreach(var s in TrainStops)
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
            if (s.DepartureTime == null && s.ArrivalTime == null)
            {
                message += $"\n���� {s.Number} δ���õ���ʱ�䣻";
            }
            if (s.Length==0)
            {
                message += $"\n���� {s.Number} ����Ϊ0��";
            }
        }
        if (message == "") return Task.FromResult(true);
        _dialogService.ShowMessage("����ʧ��", $"���� {message.Split("\n").Length-1} ������{message}\n���޸����д�����ٴγ��Ա��档");
        return Task.FromResult(false);
    }

    public static Station GenerateStation(string stationName, ObservableCollection<string> waitingAreaNames, ObservableCollection<TicketCheck> ticketChecks,ObservableCollection<TrainStop> trainStops,ObservableCollection<Platform> platforms)
    {
        var station = new Station
        {
            Name = stationName,
            WaitingAreas = [.. waitingAreaNames.Select(name => new WaitingArea { Name = name })],
            TrainStops = [.. trainStops],
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
        //List<string> stationNamesToAdd = [];
        //foreach(TrainNumber trainNumber in _databaseService.GetAllTrainNumbers())
        //{
        //    var stationNames = trainNumber.TimeTable.Select(x => x.Station).ToList();
        //    foreach(string stationName in stationNames)
        //    {
        //        if(!stationNamesToAdd.Contains(stationName) && !StationNames.Contains(stationName))
        //        {
        //            stationNamesToAdd.Add(stationName);
        //        }
        //    }
        //}
        //foreach(string stationName in stationNamesToAdd)
        //{
        //    _databaseService.AddStationByName(stationName);
        //}
        //await SaveChanges();
        //RefreshStationList();
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
                    if (TrainStops.Any(x => x.Number == firstColumn)) continue;
                    TimeSpan? departureTime = TimeSpan.Parse(data[4]);
                    TimeSpan? arrivalTime = departureTime.Value.Subtract(TimeSpan.FromMinutes(2));
                    if (data[1].Split("-")[0] == SelectedStation.Name)
                    {
                        arrivalTime = null;
                    }
                    else if (data[1].Split("-")[1] == SelectedStation.Name)
                    {
                        departureTime = null;
                    }
                    var trainStop = new TrainStop()
                    {
                        Number = firstColumn,
                        Origin = data[1].Split("-")[0],
                        Terminal = data[1].Split("-")[1],
                        ArrivalTime = arrivalTime,
                        DepartureTime = departureTime,
                        Platform = Platforms[new Random().Next(Platforms.Count)].Name,
                        Length = firstColumn.StartsWith('G') || firstColumn.StartsWith('D') || firstColumn.StartsWith('C') ? Math.Abs(firstColumn.GetHashCode()) % 3 == 0 ? 8 : 16 : 18,
                        Landmark = new[] { "��ɫ", "��ɫ", "��ɫ", "��ɫ", "��ɫ", "��ɫ", "��ɫ", null }[new Random().Next(8)]
                    };
                    trainStop.TicketChecks = [.. TicketChecks.Where(x => x.Name == trainStop.Platform + "A" || x.Name == trainStop.Platform + "B").Select(x => x.Name)];
                    if (trainStop.TicketChecks.Count == 0) trainStop.TicketChecks = [TicketChecks[new Random().Next(TicketChecks.Count)].Name];
                    TrainStops.Add(trainStop);
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
    public async Task ImportFromExcel()
    {
        if (TrainStops.Count != 0)
        {
            if (!_dialogService.GetConfirm("��ǰ�������ܻ����ʱ�̱��Ƿ������"))
            {
                return;
            }
        }
        var path = _dialogService.GetFile("Excel ������ (*.xlsx)|*.xlsx|�����ļ� (*.*)|*.*");
        if (path == null) return;
        try
        {
            ExcelPackage.License.SetNonCommercialPersonal("CRSim");
            using var package = new ExcelPackage(new FileInfo(path));
            var worksheet = package.Workbook.Worksheets[0];
            int rowCount = worksheet.Dimension.Rows;
            TrainStops.Clear();
            for (int row = 2; row <= rowCount; row++) 
            {
                if (worksheet.Cells[row, 1].Text.Trim() == ""||
                    worksheet.Cells[row, 2].Text.Trim() == "" ||
                    worksheet.Cells[row, 5].Text.Trim() == "" ||
                    worksheet.Cells[row, 6].Text.Trim() == "" ||
                    worksheet.Cells[row, 8].Text.Trim() == "" ||
                    worksheet.Cells[row, 9].Text.Trim() == "")
                {
                    continue;
                }

                TrainStops.Add(new TrainStop
                {
                    Number = worksheet.Cells[row, 1].Text.Trim(),
                    Length = int.TryParse(worksheet.Cells[row, 2].Text, out int length) ? length : 0,
                    ArrivalTime = TimeSpan.TryParseExact(worksheet.Cells[row, 3].Text, @"hh\:mm", null, out TimeSpan arrival) ? arrival : null,
                    DepartureTime = TimeSpan.TryParseExact(worksheet.Cells[row, 4].Text, @"hh\:mm", null, out TimeSpan departure) ? departure : null,
                    TicketChecks = [.. worksheet.Cells[row, 5].Text.Split(' ', StringSplitOptions.RemoveEmptyEntries)],
                    Platform = worksheet.Cells[row, 6].Text.Trim(),
                    Landmark = worksheet.Cells[row, 7].Text.Trim() == "" ? "��" : worksheet.Cells[row, 7].Text.Trim(),
                    Origin = worksheet.Cells[row, 8].Text.Trim(),
                    Terminal = worksheet.Cells[row, 9].Text.Trim()
                });
            }
        }
        catch
        {
            _dialogService.ShowMessage("����ʧ��", "�ļ���ʽ�����ռ�á�");
        }
    }
    [RelayCommand]
    public void ExportToExcel()
    {
        var path = _dialogService.SaveFile("Excel ������ (*.xlsx)|*.xlsx|�����ļ� (*.*)|*.*","data");
        if (path == null) return;
        ExcelPackage.License.SetNonCommercialPersonal("CRSim");
        using var package = new ExcelPackage();
        var worksheet = package.Workbook.Worksheets.Add("Sheet1");
        worksheet.Cells[1, 1].Value = "����";
        worksheet.Cells[1, 2].Value = "����";
        worksheet.Cells[1, 3].Value = "��ʱ";
        worksheet.Cells[1, 4].Value = "��ʱ";
        worksheet.Cells[1, 5].Value = "��Ʊ��";
        worksheet.Cells[1, 6].Value = "վ̨";
        worksheet.Cells[1, 7].Value = "�ر�";
        worksheet.Cells[1, 8].Value = "ʼ��վ";
        worksheet.Cells[1, 9].Value = "�յ�վ";
        for (int i = 0; i < TrainStops.Count; i++)
        {
            worksheet.Cells[i + 2, 1].Value = TrainStops[i].Number;
            worksheet.Cells[i + 2, 2].Value = TrainStops[i].Length;
            worksheet.Cells[i + 2, 3].Value = TrainStops[i].ArrivalTime?.ToString(@"hh\:mm") ?? string.Empty;
            worksheet.Cells[i + 2, 4].Value = TrainStops[i].DepartureTime?.ToString(@"hh\:mm") ?? string.Empty;
            worksheet.Cells[i + 2, 5].Value = TrainStops[i].TicketChecks.Count == 0 ? string.Empty : string.Join(" ", TrainStops[i].TicketChecks);
            worksheet.Cells[i + 2, 6].Value = TrainStops[i].Platform;
            worksheet.Cells[i + 2, 7].Value = TrainStops[i].Landmark;
            worksheet.Cells[i + 2, 8].Value = TrainStops[i].Origin;
            worksheet.Cells[i + 2, 9].Value = TrainStops[i].Terminal;
        }
        package.SaveAs(new FileInfo(path));
    }

    [RelayCommand]
    public void AddTrainStop()
    {
        var newTrainStop = _dialogService.GetInputTrainStop([.. TicketChecks.Select(x => x.Name)], [.. Platforms.Select(x => x.Name)]);
        if (newTrainStop != null)
        {
            if (TrainStops.Any(x => x.Number == newTrainStop.Number))
            {
                _dialogService.ShowMessage("���ʧ��", $"���� {newTrainStop.Number} �Ѵ��ڡ�");
                return;
            }
            TrainStops.Add(newTrainStop);
        }
    }
    [RelayCommand]
    public void DeleteTrainStop(object selectedTrainStop)
    {
        if (selectedTrainStop is TrainStop s)
        {
            TrainStops.Remove(s);
        }
    }
    [RelayCommand]
    public void DeleteAllTrainStop()
    {
        if (!_dialogService.GetConfirm("��ǰ������ɾ��ȫ�����Σ��Ƿ������")) return;
        TrainStops.Clear();
    }
    [RelayCommand]
    public void EditTrainStop(object _selectedTrainStop)
    {
        if (_selectedTrainStop is TrainStop selectedTrainStop)
        {
            var newTrainStop = _dialogService.GetInputTrainStop([.. TicketChecks.Select(x => x.Name)], [.. Platforms.Select(x => x.Name)], selectedTrainStop);
            if (newTrainStop != null)
            {
                TrainStops[TrainStops.IndexOf(selectedTrainStop)] = newTrainStop;
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
        if (TrainStops.Count != 0)
        {
            if (!_dialogService.GetConfirm("��ǰ���������ʱ�̱��Ƿ������"))
            {
                return;
            }
        }

        var stops = await _networkService.GetTrainNumnersAsync(SelectedStation.Name);
        if (stops.Count !=0)
        {
            TrainStops.Clear();
            foreach (TrainStop t in stops)
            {
                t.Platform = Platforms[new Random().Next(Platforms.Count)].Name;
                t.Landmark = new[] { "��ɫ", "��ɫ", "��ɫ", "��ɫ", "��ɫ", "��ɫ", "��ɫ", null }[new Random().Next(8)];
                t.Length = t.Number.StartsWith('G') || t.Number.StartsWith('D') || t.Number.StartsWith('C') ? Math.Abs(t.Number.GetHashCode()) % 3 == 0 ? 8 : 16 : 18;
                t.TicketChecks = [.. TicketChecks.Where(x => x.Name == t.Platform + "A" || x.Name == t.Platform + "B").Select(x => x.Name)];
                if (t.TicketChecks.Count == 0) t.TicketChecks = [TicketChecks[new Random().Next(TicketChecks.Count)].Name];
                TrainStops.Add(t);
            }
        }
        else
        {
            _dialogService.ShowMessage("��ȡʧ��", "��������æ��վ�����ڡ�");
        }
    }

}
