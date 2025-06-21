using CRSim.Core.Models;

namespace CRSim.Views
{
    /// <summary>
    /// UserInputDialog.xaml 的交互逻辑
    /// </summary>
    public partial class TrainStopDialog : Window
    {
        public class ticketCheck
        {
            public string TicketCheck { get; set; }
            public bool Checked { get; set; } = false;
        }

        public ObservableCollection<string> Landmarks { get; set; } = ["无", "红色", "绿色", "褐色", "蓝色", "紫色", "黄色", "橙色"];

        public TrainStop GeneratedTrainStop;

        public ObservableCollection<ticketCheck> TicketChecksList { get; set; } = [];

        public ObservableCollection<string> Platforms { get; set; } = [];

        public TrainStopDialog(List<string> ticketChecks,List<string> platforms)
        {
            DataContext = this;
            WindowChrome.SetWindowChrome(
                this,
                new WindowChrome
                {
                    CaptionHeight = 50,
                    CornerRadius = default,
                    GlassFrameThickness = new Thickness(-1),
                    ResizeBorderThickness = ResizeMode == ResizeMode.NoResize ? default : new Thickness(4),
                    UseAeroCaptionButtons = true
                }
            );
            InitializeComponent();
            IntermediateStation.IsChecked = true;
            LengthTextBox.Text = "16";
            TicketChecksList.Clear();
            foreach (string s in ticketChecks)
            {
                TicketChecksList.Add(new ticketCheck()
                {
                    TicketCheck = s,
                    Checked = false
                });
            }
            foreach (string s in platforms)
            {
                Platforms.Add(s);
            }
            ValidateInput();
        }

        public TrainStopDialog(List<string> ticketChecks, List<string> platforms, TrainStop trainStop)
        {
            DataContext = this;
            WindowChrome.SetWindowChrome(
                this,
                new WindowChrome
                {
                    CaptionHeight = 50,
                    CornerRadius = default,
                    GlassFrameThickness = new Thickness(-1),
                    ResizeBorderThickness = ResizeMode == ResizeMode.NoResize ? default : new Thickness(4),
                    UseAeroCaptionButtons = true
                }
            );
            InitializeComponent();
            NumberTextBox.IsEnabled = false;
            NumberTextBox.Text = trainStop.Number;
            ArrivalTextBox.Text = trainStop.Terminal;
            DepartureTextBox.Text = trainStop.Origin;
            LengthTextBox.Text = trainStop.Length.ToString();
            StationKindPanel.IsEnabled = false;
            if (trainStop.ArrivalTime != null)
            {
                StartHour.Text = trainStop.ArrivalTime.Value.Hours.ToString();
                StartMinute.Text = trainStop.ArrivalTime.Value.Minutes.ToString();
            }
            else
            {
                StartingStation.IsChecked = true;
            }
            if (trainStop.DepartureTime != null)
            {
                EndHour.Text = trainStop.DepartureTime.Value.Hours.ToString();
                EndMinute.Text = trainStop.DepartureTime.Value.Minutes.ToString();
            }
            else
            {
                FinalStation.IsChecked = true;
            }
            if(!StartingStation.IsChecked.Value && !FinalStation.IsChecked.Value)
            {
                IntermediateStation.IsChecked = true;
            }
            TicketChecksList.Clear();
            foreach (string s in ticketChecks)
            {
                TicketChecksList.Add(new ticketCheck()
                {
                    TicketCheck = s,
                    Checked = trainStop.TicketChecks.Contains(s)
                });
            }
            foreach (string s in platforms)
            {
                Platforms.Add(s);
            }
            PlatformComboBox.SelectedIndex = Platforms.IndexOf(trainStop.Platform);
            LandmarkComboBox.SelectedIndex = trainStop.Landmark == null ? 0 : Landmarks.IndexOf(trainStop.Landmark);
            ValidateInput();
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            GeneratedTrainStop = new TrainStop
            {
                Number = NumberTextBox.Text,
                Terminal = ArrivalTextBox.Text,
                Origin = DepartureTextBox.Text,
                ArrivalTime = StartHour.IsEnabled ? TimeSpan.Parse($"{StartHour.Text}:{StartMinute.Text}") : null,
                DepartureTime = EndHour.IsEnabled ? TimeSpan.Parse($"{EndHour.Text}:{EndMinute.Text}") : null,
                WaitingArea = TicketChecksList.Where(x => x.Checked).FirstOrDefault().TicketCheck.Split(" - ")[0],
                TicketChecks = [.. TicketChecksList.Where(x => x.Checked).Select(x => x.TicketCheck.Split(" - ")[1])],
                Platform = (string)PlatformComboBox.SelectedItem,
                Length = int.Parse(LengthTextBox.Text),
                Landmark = (string)LandmarkComboBox.SelectedItem == "无" ? null : (string)LandmarkComboBox.SelectedItem,
            };
            DialogResult = true;
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void StartHour_TextChanged(object sender, TextChangedEventArgs e)
        {
            ValidateInput();
            if (StartHour.Text.Length == 2) StartMinute.Focus();
        }

        private void StartMinute_TextChanged(object sender, TextChangedEventArgs e)
        {
            ValidateInput();
            if (StartMinute.Text.Length == 2) EndHour.Focus();
        }

        private void EndHour_TextChanged(object sender, TextChangedEventArgs e)
        {
            ValidateInput();
            if (EndHour.Text.Length == 2) EndMinute.Focus();
        }

        private void EndMinute_TextChanged(object sender, TextChangedEventArgs e)
        {
            ValidateInput();
            if (EndMinute.Text.Length == 2) AccentButton.Focus();
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            ValidateInput();
        }

        private void CheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            ValidateInput();
        }

        private static bool ValidateTime(string input, int maxValue)
        {
            if (!string.IsNullOrWhiteSpace(input) && int.TryParse(input, null, out int time))
            {
                return 0 <= time && time < maxValue;
            }
            return false;
        }

        private void ValidateInput()
        {
            // 检查四个文本框是否非空
            bool areTextBoxesFilled =
                (!StartHour.IsEnabled || ValidateTime(StartHour.Text, 24)) &&
                (!StartMinute.IsEnabled || ValidateTime(StartMinute.Text, 60)) &&
                (!EndHour.IsEnabled || ValidateTime(EndHour.Text, 24)) &&
                (!EndMinute.IsEnabled || ValidateTime(EndMinute.Text, 60));

            AccentButton.IsEnabled =
                (!TicketChecksCheckList.IsEnabled ||
                (TicketChecksList.Any(x => x.Checked) && TicketChecksList.Where(x=>x.Checked).Select(x=>x.TicketCheck.Split(" - ")[0]).Distinct().Count() <= 1)) &&
                areTextBoxesFilled &&
                !string.IsNullOrWhiteSpace(NumberTextBox.Text) &&
                !string.IsNullOrWhiteSpace(ArrivalTextBox.Text) &&
                !string.IsNullOrWhiteSpace(DepartureTextBox.Text) &&
                int.TryParse(LengthTextBox.Text, out int i) && i > 0 &&
                PlatformComboBox.SelectedItem != null;
        }

        private T FindVisualChild<T>(DependencyObject obj) where T : DependencyObject
        {
            if (obj == null)
                return null;

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(obj); i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(obj, i);
                if (child is T t)
                {
                    return t;
                }
                else
                {
                    T childOfChild = FindVisualChild<T>(child);
                    if (childOfChild != null)
                        return childOfChild;
                }
            }
            return null;
        }

        private void StartingStation_Checked(object sender, RoutedEventArgs e)
        {
            if (StartHour == null) return;
            StartHour.IsEnabled = false;
            StartMinute.IsEnabled = false;
            EndHour.IsEnabled = true;
            EndMinute.IsEnabled = true;
            TicketChecksCheckList.IsEnabled = true;
        }

        private void IntermediateStation_Checked(object sender, RoutedEventArgs e)
        {
            if (StartHour == null) return;
            StartHour.IsEnabled = true;
            StartMinute.IsEnabled = true;
            EndHour.IsEnabled = true;
            EndMinute.IsEnabled = true;
            TicketChecksCheckList.IsEnabled = true;
        }

        private void FinalStation_Checked(object sender, RoutedEventArgs e)
        {
            if (StartHour == null) return;
            StartHour.IsEnabled = true;
            StartMinute.IsEnabled = true;
            EndHour.IsEnabled = false;
            EndMinute.IsEnabled = false;
            TicketChecksCheckList.IsEnabled = false;
        }

        private void TicketChecksCheckList_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            var eventArg = new MouseWheelEventArgs(e.MouseDevice, e.Timestamp, e.Delta)
            {
                RoutedEvent = UIElement.MouseWheelEvent,
                Source = sender
            };
            TicketChecksCheckList.RaiseEvent(eventArg);
        }

        private void NumberTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            ValidateInput();
        }
    }
}