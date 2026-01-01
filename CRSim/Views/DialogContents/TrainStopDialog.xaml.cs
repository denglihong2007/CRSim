namespace CRSim.Views.DialogContents;
public sealed partial class TrainStopDialog : Page
{
    public TrainStop GeneratedTrainStop;

    public List<string> Landmarks { get; set; } = ["无", "红色", "绿色", "褐色", "蓝色", "紫色", "黄色", "橙色"];

    public List<string> Platforms { get; set; } = [];

    public List<WaitingArea> WaitingAreas { get; set; }

    private readonly Action<bool> _onValidityChanged;

    public TrainStopDialog(List<WaitingArea> waitingAreas, List<string> platforms, Action<bool> onValidityChanged)
    {
        WaitingAreas = waitingAreas;
        foreach (string s in platforms)
        {
            Platforms.Add(s);
        }
        InitializeComponent();
        _onValidityChanged = onValidityChanged;
        Validate(null, null);
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
            SuspendToggleSwitch.IsOn = true;
        }
        else
        {
            StatusTextBox.Text = trainStop.Status.Value.TotalMinutes.ToString();
        }
        if (trainStop.TicketCheckIds != null)
        {
            foreach (var id in trainStop.TicketCheckIds)
            {
                //WaitingAreasList.SelectedItems.Add(waitingAreas.SelectMany(x => x.TicketChecks).FirstOrDefault(x => x.Id == id));
            }
        }

        Validate(null, null);
    }
    private static bool ValidateTime(string input, int maxValue)
    {
        if (!string.IsNullOrWhiteSpace(input) && int.TryParse(input, null, out int time))
        {
            return 0 <= time && time < maxValue;
        }
        return false;
    }
    private void Validate(object sender, object e)
    {
        if (StartHour is null) return;
        bool areTextBoxesFilled =
            (!StartHour.IsEnabled || ValidateTime(StartHour.Text, 24)) &&
            (!StartMinute.IsEnabled || ValidateTime(StartMinute.Text, 60)) &&
            (!EndHour.IsEnabled || ValidateTime(EndHour.Text, 24)) &&
            (!EndMinute.IsEnabled || ValidateTime(EndMinute.Text, 60));
        bool isValid =
            (!WaitingAreasList.IsEnabled ||
            WaitingAreasList.SelectedItems?.Count != 0) &&
            areTextBoxesFilled &&
            !string.IsNullOrWhiteSpace(NumberTextBox.Text) &&
            !string.IsNullOrWhiteSpace(ArrivalTextBox.Text) &&
            !string.IsNullOrWhiteSpace(DepartureTextBox.Text) &&
            int.TryParse(LengthTextBox.Text, out int i) && i > 0 &&
            PlatformComboBox.SelectedItem != null &&
            int.TryParse(StatusTextBox.Text, out int m);
        _onValidityChanged?.Invoke(isValid);
    }

    private void StartHour_TextChanged(object sender, TextChangedEventArgs e)
    {
        Validate(null, null);
        if (StartHour.Text.Length == 2) StartMinute.Focus(FocusState.Programmatic);
    }

    private void StartMinute_TextChanged(object sender, TextChangedEventArgs e)
    {
        Validate(null, null);
        if (StartMinute.Text.Length == 2) EndHour.Focus(FocusState.Programmatic);
    }

    private void EndHour_TextChanged(object sender, TextChangedEventArgs e)
    {
        Validate(null, null);
        if (EndHour.Text.Length == 2) EndMinute.Focus(FocusState.Programmatic);
    }
    public void GenerateTrainStop()
    {
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
            Status = SuspendToggleSwitch.IsOn ? null : TimeSpan.FromMinutes(int.Parse(StatusTextBox.Text))
        };
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
            Validate(null, null);
        }
    }
}