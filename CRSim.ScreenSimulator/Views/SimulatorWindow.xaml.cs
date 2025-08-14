using CRSim.Core.Abstractions;
using CRSim.ScreenSimulator.Models;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace CRSim.ScreenSimulator.Views
{
    public partial class SimulatorWindow : Window
    {
        private readonly ITimeService _timeService;
        private readonly StyleManager _styleManager;
        public Session Session { get; }
        public SimulatorWindow(Page page,ITimeService timeService,StyleManager styleManager,Session session)
        {
            InitializeComponent();
            RenderOptions.SetEdgeMode(this, EdgeMode.Aliased);
            Session = session;
            contentFrame.Content = page;
            _timeService = timeService;
            _styleManager = styleManager;

            foreach (var resource in page.Resources.Values)
            {
                if (resource is IHasTimeService needsTime)
                {
                    needsTime.TimeService = timeService;
                }
            }
            _timeService.OneSecondElapsed += UpdateTime;
        }

        private void UpdateTime(object? sender, EventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                TimeText.Text = _timeService.GetDateTimeNow().ToString("HH:mm:ss");
            });
        }

        private void Window_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left) DragMove();
        }

        private void DecreaseSpeed(object sender, MouseButtonEventArgs e)
        {
            if (_timeService.Speed > 0.25 && _timeService.Speed <= 1024)
            {
                _timeService.Speed /= 2;
                SpeedText.Text = _timeService.Speed.ToString() + "x";
                if (_timeService.Speed == 1)
                {
                    RestoreSpeedIcon.Visibility = Visibility.Hidden;
                }
                else
                {
                    RestoreSpeedIcon.Visibility = Visibility.Visible;
                }
            }
        }
        private void IncreaseSpeed(object sender, MouseButtonEventArgs e)
        {
            if (_timeService.Speed >= 0.25 && _timeService.Speed < 1024)
            {
                _timeService.Speed *= 2;
                SpeedText.Text = _timeService.Speed.ToString() + "x";
                if (_timeService.Speed == 1)
                {
                    RestoreSpeedIcon.Visibility = Visibility.Hidden;
                }
                else
                {
                    RestoreSpeedIcon.Visibility = Visibility.Visible;
                }
            }
        }

        private void RestoreSpeed(object sender, MouseButtonEventArgs e)
        {
            _timeService.Speed = 1;
            SpeedText.Text = "1x";
            RestoreSpeedIcon.Visibility = Visibility.Hidden;
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            _styleManager.DispatcherQueue.TryEnqueue(() =>
            {
                _styleManager.ActiveWindows.Remove(this);
            });
        }

        public void FoucsWindow(object o,object s)
        {
            Dispatcher.Invoke(() =>
            {
                WindowState = WindowState.Normal;
                Activate();
                Topmost = true;
                Topmost = false;
                Focus();
            });
        }
        public void CloseWindow(object o, object s)
        {
            Dispatcher.Invoke(() =>
            {
                Close();
            });
        }

        private void FullScreen(object sender, MouseButtonEventArgs e)
        {
            Left = 0;
            viewbox.Stretch = Stretch.Uniform;
            FullScreenIcon.Visibility = Visibility.Collapsed;
            RestoreWindowIcon.Visibility = Visibility.Visible;
        }

        private void RestoreWindow(object sender, MouseButtonEventArgs e)
        {
            viewbox.Stretch = Stretch.None;
            FullScreenIcon.Visibility = Visibility.Visible;
            RestoreWindowIcon.Visibility = Visibility.Collapsed;
        }
    }
}
