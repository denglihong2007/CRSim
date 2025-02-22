using System.Collections.Specialized;
using System.IO;

namespace CRSim.ViewModels;

public partial class TrainNumberManagementPageViewModel : ObservableObject 
{
	[ObservableProperty]
	private string _pageTitle = "���ι���";

	[ObservableProperty]
	private string _pageDescription = "���������Ǻ���ܰ�����ѣ�����ǵñ���! Owo";

    [ObservableProperty]
    private bool _isSelected = false;

    [ObservableProperty]
    private List<string> _numbersList = [];

    [ObservableProperty]
    private TrainNumber _selectedTrainNumber = new();

    [ObservableProperty]
    private string _runningTime;

    [ObservableProperty]
    private int _progressValue = 0;

    public ObservableCollection<TrainStop> TrainStopsList { get; private set; }= [];

    private readonly IDatabaseService _databaseService;

    private readonly IDialogService _dialogService;

    private readonly INetworkService _networkService;

    public TrainNumberManagementPageViewModel(IDatabaseService databaseService,IDialogService dialogService,INetworkService networkService)
    {
		_databaseService = databaseService;
        _dialogService = dialogService;
        _networkService = networkService;
        RefreshNumbersList();
        TrainStopsList.CollectionChanged += RefreshTrainNumberInfo;
    }
    private void RefreshTrainNumberInfo(object? s,NotifyCollectionChangedEventArgs args)
    {
        if (SelectedTrainNumber != null && TrainStopsList.Count >= 2)
        {
            var timeSpan = (TrainStopsList.Last().ArrivalTime - TrainStopsList.First().DepartureTime).Value;
            RunningTime = $"{timeSpan.Days}�� {timeSpan.Hours}Сʱ {timeSpan.Minutes}����";
        }
    }

    public void RefreshNumbersList()
    {
        var trainNumbersList = _databaseService.GetAllTrainNumbers();
        NumbersList = new List<string>(trainNumbersList.Select(s => s.Number));
    }

    [RelayCommand]
    public void NumberSelected(object selectedNumber)
    {
        if(selectedNumber is string v)
        {
            SelectedTrainNumber = _databaseService.GetTrainNumberByNumber(v); 

            TrainStopsList.Clear();
            foreach (TrainStop t in SelectedTrainNumber.TimeTable)
            {
                TrainStopsList.Add(t);
            }
            IsSelected = true;
        }
        if(selectedNumber == null)
        {
            IsSelected = false;
        }
    }
    [RelayCommand]
    public async void GetTrainStopsOnline() 
    {
        if (TrainStopsList.Count != 0)
        {
            if (!_dialogService.GetConfirm("��ǰ�����������ʱ�̱��Ƿ������"))
            {
                return;
            }
        }

        var stops = await _networkService.GetTrainStopsAsync(SelectedTrainNumber.Number);
        if (stops != null)
        {
            TrainStopsList.Clear();
            foreach (TrainStop t in stops)
            {
                TrainStopsList.Add(t);
            }
        }
        else
        {
            _dialogService.ShowMessage("��ȡʧ��", "δ�ҵ��ó��Ρ�");
        }
    }
    [RelayCommand]
    public void AddTrainStop(object selectedTrainStop)
    {
        var newTrainStop = _dialogService.GetInputTrainStop();
        if (newTrainStop != null)
        {
            if (TrainStopsList.Any(x => x.Station == newTrainStop.Station))
            {
                _dialogService.ShowMessage("���ʧ��", $"ͣվ {newTrainStop.Station} �Ѵ��ڡ�");
                return;
            }
            if (TrainStopsList.Any(x => x.ArrivalTime == null) && newTrainStop.ArrivalTime == null)
            {
                _dialogService.ShowMessage("���ʧ��", $"ʼ��վ�Ѵ��ڡ�");
                return;
            }
            if (TrainStopsList.Any(x => x.DepartureTime == null) && newTrainStop.DepartureTime == null)
            {
                _dialogService.ShowMessage("���ʧ��", $"�յ�վ�Ѵ��ڡ�");
                return;
            }
            if (selectedTrainStop != null && newTrainStop.DepartureTime == null && TrainStopsList.IndexOf((TrainStop)selectedTrainStop) != TrainStopsList.Count-1)
            {
                _dialogService.ShowMessage("���ʧ��", $"�յ�վֻ�ܼ���ʱ�̱�ĩλ��");
                return;
            }
            if(TrainStopsList.Count==0 && newTrainStop.ArrivalTime != null)
            {
                _dialogService.ShowMessage("���ʧ��", $"ʱ�̱���λֻ����ʼ��վ��");
                return;
            }
            if (selectedTrainStop != null)
            {
                TrainStopsList.Insert(TrainStopsList.IndexOf((TrainStop)selectedTrainStop)+1, newTrainStop);
            }
            else
            {
                TrainStopsList.Add(newTrainStop);
            }
        }
    }
    [RelayCommand]
    public void DeleteTrainStop(object selectedTrainStop)
    {
        if(selectedTrainStop is TrainStop s)
        {
            TrainStopsList.Remove(s);
        }
    }
    [RelayCommand]
    public void EditTrainStop(object _selectedTrainStop)
    {
        if (_selectedTrainStop is TrainStop selectedTrainStop)
        {
            var newTrainStop = _dialogService.GetInputTrainStop(selectedTrainStop);
            if (newTrainStop != null)
            {
                TrainStopsList[TrainStopsList.IndexOf(selectedTrainStop)] = newTrainStop;
            }
        }
    }
    [RelayCommand]
    public async Task AddTrainNumber()
    {
        string? number = _dialogService.GetInput("�����복�κ�").ToUpper();
        if (number != null)
        {
            if (NumbersList.Contains(number))
            {
                _dialogService.ShowMessage("���ʧ��", $"���κ� {number} �Ѵ��ڡ�");
                return;
            }
            _databaseService.AddTrainNumber(number);
            await _databaseService.SaveData();
            RefreshNumbersList();
        }
    }
    [RelayCommand]
    public async Task DeleteTrainNumber(object selectedTrainNumber)
    {
        if (selectedTrainNumber is string s && !String.IsNullOrEmpty(s))
        {
            _databaseService.DeleteTrainNumber(_databaseService.GetTrainNumberByNumber(s));
            await _databaseService.SaveData();
            RefreshNumbersList();
        }
    }
    [RelayCommand]
    public async Task DeleteAllTrainNumbers()
    {
        if (!_dialogService.GetConfirm("��ǰ������ɾ��ȫ�����Σ��Ƿ������")) return;
        var trainNumbers = _databaseService.GetAllTrainNumbers().ToList();
        foreach (TrainNumber trainNumber in trainNumbers)
        {
            _databaseService.DeleteTrainNumber(trainNumber);
        }
        await _databaseService.SaveData();
        RefreshNumbersList();
    }
    [RelayCommand]
    public async Task SaveChanges()
    {
        if (TrainStopsList.First().ArrivalTime != null)
        {
            _dialogService.ShowMessage("����ʧ��", "ʱ�̱���λ������ʼ��վ��");
            return;
        }
        if (TrainStopsList.Last().DepartureTime != null)
        {
            _dialogService.ShowMessage("����ʧ��", "ʱ�̱�ĩλ�������յ�վ��");
            return;
        }
        _databaseService.UpdateTimeTable(SelectedTrainNumber, [.. TrainStopsList]);
        await _databaseService.SaveData();
    }
    [RelayCommand]
    public async void ImportFromLulutong()
    {
        var path = _dialogService.GetFile("CSV �ļ� (*.csv)|*.csv|�����ļ� (*.*)|*.*");
        if (path == null) return;
        try
        {
            var numbers = ReadCsvFirstColumn(path);
            var intersection = numbers.Intersect(NumbersList).ToList();
            if (intersection.Count != 0)
            {
                if (!_dialogService.GetConfirm("��ǰ�������ܸ������г������ã��Ƿ������")) return;
            }
            foreach (string s in intersection)
            {
                _databaseService.DeleteTrainNumber(_databaseService.GetTrainNumberByNumber(s));
            }
            RefreshNumbersList();
            int total = numbers.Count;
            int current = 0;
            foreach (string number in numbers)
            {
                current++;
                ProgressValue = (int)((double)current / total * 100);
                List<TrainStop>? timeTable = await _networkService.GetTrainStopsAsync(number);
                await Task.Delay(1000);
                timeTable ??= [];
                _databaseService.AddTrainNumber(number);
                _databaseService.UpdateTimeTable(_databaseService.GetTrainNumberByNumber(number), timeTable);
            }
            ProgressValue = 0;
            await _databaseService.SaveData();
            RefreshNumbersList();
        }
        catch
        {
            _dialogService.ShowMessage("����ʧ��", "�ļ���ʽ�����ռ�á�");
        }
    }
    private static List<string> ReadCsvFirstColumn(string filePath)
    {
        var firstColumnData = new List<string>();

        // ʹ�� StreamReader ��ȡ�ļ�
        using (var reader = new StreamReader(filePath))
        {
            while (!reader.EndOfStream)
            {
                var line = reader.ReadLine(); // ��ȡÿһ��
                var columns = line.Split(','); // ���ն��ŷָ���

                if (columns.Length > 0)
                {
                    var firstColumn = columns[0].Trim(); // ��ȡ��ȥ����β�ո�

                    if (!string.IsNullOrEmpty(firstColumn) && (char.IsLetterOrDigit(firstColumn[0])))
                    {
                        int spaceIndex = firstColumn.IndexOf(' ');
                        if (spaceIndex != -1)
                        {
                            firstColumn = firstColumn[..spaceIndex]; // ȡ�ո�ǰ�Ĳ���
                        }

                        if (firstColumnData.Contains(firstColumn)) continue;
                        firstColumnData.Add(firstColumn); // ��ӵ�����б�
                    }
                }
            }
        }
        return firstColumnData;
    }
}
