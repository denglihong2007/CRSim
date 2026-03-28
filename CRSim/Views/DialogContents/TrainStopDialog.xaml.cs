namespace CRSim.Views.DialogContents;
public sealed partial class TrainStopDialog : Page
{
    public TrainStop GeneratedTrainStop;

    public List<string> Landmarks { get; set; } = ["无", "红色", "绿色", "褐色", "蓝色", "紫色", "黄色", "橙色"];

    public List<string> Platforms { get; set; } = [];

    public List<WaitingArea> WaitingAreas { get; set; }

    private readonly Action<bool> _onValidityChanged;

    private TrainStop originalTrainStop = new TrainStop();

    public TrainStopDialog(List<WaitingArea> waitingAreas, List<string> platforms, Action<bool> onValidityChanged)
    {
        WaitingAreas = waitingAreas;
        foreach (string s in platforms)
        {
            Platforms.Add(s);
        }
        InitializeComponent();
        _onValidityChanged = onValidityChanged;
        StatusComboBox.SelectedIndex = 0;
        UpdateStatusInputState();
        Validate(this, EventArgs.Empty);
    }
    public TrainStopDialog(List<WaitingArea> waitingAreas, List<string> platforms, TrainStop trainStop, Action<bool> onValidityChanged)
    {
        WaitingAreas = waitingAreas;
        foreach (string s in platforms)
        {
            Platforms.Add(s);
        }
        InitializeComponent();
        _onValidityChanged = onValidityChanged;
        NumberTextBox.Text = trainStop.Number;
        LengthTextBox.Text = trainStop.Length.ToString();
        ArrivalTextBox.Text = trainStop.Terminal;
        DepartureTextBox.Text = trainStop.Origin;
        PlatformComboBox.SelectedItem = trainStop.Platform;
        LandmarkComboBox.SelectedItem = trainStop.Landmark ?? "无";
        if (trainStop.ArrivalTime.HasValue)
        {
            StartHour.Text = trainStop.ArrivalTime.Value.Hours.ToString("D2");
            StartMinute.Text = trainStop.ArrivalTime.Value.Minutes.ToString("D2");
        }
        else
        {
            StartHour.IsEnabled = false;
            StartMinute.IsEnabled = false;
            StationKindPanelRadioButtons.SelectedIndex = 0;
        }
        if (trainStop.DepartureTime.HasValue)
        {
            EndHour.Text = trainStop.DepartureTime.Value.Hours.ToString("D2");
            EndMinute.Text = trainStop.DepartureTime.Value.Minutes.ToString("D2");
        }
        else
        {
            EndHour.IsEnabled = false;
            EndMinute.IsEnabled = false;
            StationKindPanelRadioButtons.SelectedIndex = 2;
        }

        if (trainStop.Status is null)
        {
            StatusComboBox.SelectedItem = "停运";
            StatusMinutesTextBox.Text = "0";
        }
        else if (TrainStatus.IsDelayUnknown(trainStop.Status))
        {
            StatusComboBox.SelectedItem = "晚点未定";
            StatusMinutesTextBox.Text = "0";
        }
        else if (trainStop.Status.Value > TimeSpan.Zero)
        {
            StatusComboBox.SelectedItem = "晚点";
            StatusMinutesTextBox.Text = Math.Abs((int)Math.Round(trainStop.Status.Value.TotalMinutes)).ToString();
        }
        else if (trainStop.Status.Value < TimeSpan.Zero)
        {
            StatusComboBox.SelectedItem = "早点";
            StatusMinutesTextBox.Text = Math.Abs((int)Math.Round(trainStop.Status.Value.TotalMinutes)).ToString();
        }
        else
        {
            StatusComboBox.SelectedItem = "正点";
            StatusMinutesTextBox.Text = "0";
        }

        UpdateStatusInputState();
        Validate(this, EventArgs.Empty);
        originalTrainStop = trainStop;
    }
    private static bool ValidateTime(string input, int maxValue)
    {
        if (!string.IsNullOrWhiteSpace(input) && int.TryParse(input, null, out int time))
        {
            return 0 <= time && time < maxValue;
        }
        return false;
    }

    private bool IsEarlyOrLateSelected()
    {
        if (StatusComboBox?.SelectedItem is not string status) return false;
        return status == "早点" || status == "晚点";
    }

    private void Validate(object sender, object e)
    {
        if (StartHour is null) return;
        bool areTextBoxesFilled =
            (!StartHour.IsEnabled || ValidateTime(StartHour.Text, 24)) &&
            (!StartMinute.IsEnabled || ValidateTime(StartMinute.Text, 60)) &&
            (!EndHour.IsEnabled || ValidateTime(EndHour.Text, 24)) &&
            (!EndMinute.IsEnabled || ValidateTime(EndMinute.Text, 60));

        bool isStatusValid =
            !IsEarlyOrLateSelected() ||
            (int.TryParse(StatusMinutesTextBox.Text, out int minutes) && minutes > 0);

        bool isValid =
            (!WaitingAreasList.IsEnabled ||
            WaitingAreasList.SelectedItems?.Count != 0) &&
            areTextBoxesFilled &&
            !string.IsNullOrWhiteSpace(NumberTextBox.Text) &&
            !string.IsNullOrWhiteSpace(ArrivalTextBox.Text) &&
            !string.IsNullOrWhiteSpace(DepartureTextBox.Text) &&
            int.TryParse(LengthTextBox.Text, out int i) && i > 0 &&
            PlatformComboBox.SelectedItem != null &&
            StatusComboBox.SelectedItem != null &&
            isStatusValid;
        _onValidityChanged?.Invoke(isValid);
    }

    private void StartHour_TextChanged(object sender, TextChangedEventArgs e)
    {
        Validate(this, EventArgs.Empty);
        if (StartHour.Text.Length == 2) StartMinute.Focus(FocusState.Programmatic);
    }

    private void StartMinute_TextChanged(object sender, TextChangedEventArgs e)
    {
        Validate(this, EventArgs.Empty);
        if (StartMinute.Text.Length == 2) EndHour.Focus(FocusState.Programmatic);
    }

    private void EndHour_TextChanged(object sender, TextChangedEventArgs e)
    {
        Validate(this, EventArgs.Empty);
        if (EndHour.Text.Length == 2) EndMinute.Focus(FocusState.Programmatic);
    }

    private void StatusComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        UpdateStatusInputState();
        Validate(this, EventArgs.Empty);
    }

    public void GenerateTrainStop()
    {
        var selectedStatus = StatusComboBox.SelectedItem as string ?? "正点";
        TimeSpan? statusValue = selectedStatus switch
        {
            "停运" => null,
            "晚点未定" => TrainStatus.DelayUnknown,
            "晚点" => TimeSpan.FromMinutes(int.Parse(StatusMinutesTextBox.Text)),
            "早点" => -TimeSpan.FromMinutes(int.Parse(StatusMinutesTextBox.Text)),
            _ => TimeSpan.Zero
        };

        GeneratedTrainStop = new TrainStop
        {
            Number = NumberTextBox.Text,
            Terminal = ArrivalTextBox.Text,
            Origin = DepartureTextBox.Text,
            ArrivalTime = StartHour.IsEnabled ? TimeSpan.Parse($"{StartHour.Text}:{StartMinute.Text}") : null,
            DepartureTime = EndHour.IsEnabled ? TimeSpan.Parse($"{EndHour.Text}:{EndMinute.Text}") : null,
            TicketCheckIds = EndHour.IsEnabled ? [.. WaitingAreasList.SelectedItems
                .OfType<TicketCheck>()
                .Select(x => x.Id)] : null,
            Platform = (string)PlatformComboBox.SelectedItem,
            Length = int.Parse(LengthTextBox.Text),
            Landmark = (string)LandmarkComboBox.SelectedItem == "无" ? null : (string)LandmarkComboBox.SelectedItem,
            Status = statusValue
        };
    }

    private void UpdateStatusInputState()
    {
        if (StatusMinutesTextBox is null || StatusComboBox is null) return;
        StatusMinutesTextBox.IsEnabled = IsEarlyOrLateSelected();
    }

    private void StationKindPanelRadioButtons_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (StartHour == null) return;
        if(sender is RadioButtons rb)
        {
            string selectedType = rb.SelectedItem as string;
            switch (selectedType)
            {
                case "始发站":
                    StartHour.IsEnabled = false;
                    StartMinute.IsEnabled = false;
                    EndHour.IsEnabled = true;
                    EndMinute.IsEnabled = true;
                    WaitingAreasList.IsEnabled = true;
                    break;
                case "中间站":
                    StartHour.IsEnabled = true;
                    StartMinute.IsEnabled = true;
                    EndHour.IsEnabled = true;
                    EndMinute.IsEnabled = true;
                    WaitingAreasList.IsEnabled = true;
                    break;
                case "终到站":
                    StartHour.IsEnabled = true;
                    StartMinute.IsEnabled = true;
                    EndHour.IsEnabled = false;
                    EndMinute.IsEnabled = false;
                    WaitingAreasList.IsEnabled = false;
                    break;
            }
            Validate(this, EventArgs.Empty);
        }
    }

    private void WaitingAreasList_Loaded(object sender, RoutedEventArgs e)
    {
        if (originalTrainStop.TicketCheckIds != null)
        {
            WaitingAreasList.SelectedItems.Clear();
            foreach (var area in WaitingAreas)
            {
                var matchedChecks = area.TicketChecks
                                        .Where(tc => originalTrainStop.TicketCheckIds.Contains(tc.Id))
                                        .ToList();

                if (matchedChecks.Count != 0)
                {
                    if (matchedChecks.Count == area.TicketChecks.Count)
                    {
                        WaitingAreasList.SelectedItems.Add(area);
                    }
                    foreach (var check in matchedChecks)
                    {
                        WaitingAreasList.SelectedItems.Add(check);
                    }
                }
            }
            Validate(this, EventArgs.Empty);
        }

    }
}
