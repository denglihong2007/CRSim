namespace CRSim.Views.DialogContents;
public sealed partial class TrainNumberStopDialog : Page
{
    public TrainStop GeneratedTrainStop;

    private readonly Action<bool> _onValidityChanged;

    public TrainNumberStopDialog(Action<bool> onValidityChanged)
    {
        InitializeComponent();
        _onValidityChanged = onValidityChanged;
        Validate(null, null);
    }
    public TrainNumberStopDialog(TrainStop trainStop, Action<bool> onValidityChanged)
    {
        InitializeComponent();
        _onValidityChanged = onValidityChanged;
        NumberTextBox.Text = trainStop.Station;
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
            areTextBoxesFilled &&
            !string.IsNullOrWhiteSpace(NumberTextBox.Text);
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
            Station = NumberTextBox.Text,
            ArrivalTime = StartHour.IsEnabled ? TimeSpan.Parse($"{StartHour.Text}:{StartMinute.Text}") : null,
            DepartureTime = EndHour.IsEnabled ? TimeSpan.Parse($"{EndHour.Text}:{EndMinute.Text}") : null,
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
                    break;
                case "中间站":
                    StartHour.IsEnabled = true;
                    StartMinute.IsEnabled = true;
                    EndHour.IsEnabled = true;
                    EndMinute.IsEnabled = true;
                    break;
                case "终到站":
                    StartHour.IsEnabled = true;
                    StartMinute.IsEnabled = true;
                    EndHour.IsEnabled = false;
                    EndMinute.IsEnabled = false;
                    break;
            }
            Validate(null, null);
        }
    }
}