using System.ComponentModel;

namespace CRSim.Views
{
    // �����¼�ί������
    public delegate void SplashScreenClosedEventHandler(object sender, EventArgs e);

    public sealed partial class StartWindow : Window, INotifyPropertyChanged
    {
        // �����¼�
        public event SplashScreenClosedEventHandler SplashScreenClosed;
        public string AppVersion { get; set; } = App.AppVersion;

        private string _status = "��������...";
        public string Status
        {
            get => _status;
            set
            {
                if (_status != value)
                {
                    _status = value;
                    OnPropertyChanged(nameof(Status));
                }
            }
        }

        public event System.ComponentModel.PropertyChangedEventHandler? PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
        }

        public StartWindow()
        {
            this.InitializeComponent();

            AppWindow.Resize(new Windows.Graphics.SizeInt32(750, 270));
            var area = DisplayArea.GetFromWindowId(AppWindow.Id, DisplayAreaFallback.Nearest)?.WorkArea;
            if (area != null)
            {
                AppWindow.Move(new PointInt32((area.Value.Width - AppWindow.Size.Width) / 2, (area.Value.Height - AppWindow.Size.Height) / 2));
            }


            OverlappedPresenter presenter = OverlappedPresenter.Create();

            presenter.IsAlwaysOnTop = true;
            presenter.IsMaximizable = false;
            presenter.IsMinimizable = false;
            presenter.IsResizable = false;
            presenter.SetBorderAndTitleBar(true, false);

            AppWindow.SetPresenter(presenter);

            AppWindow.MoveInZOrderAtTop();

            Logo.MediaPlayer.Play();
            

        }

        // ��ɳ�ʼ�����������������ر�
        public void CompleteInitialization()
        {
            SplashScreenClosed?.Invoke(this, EventArgs.Empty);
            this.Close();
        }
    }
}
