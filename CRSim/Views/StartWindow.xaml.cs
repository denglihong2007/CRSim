using System.Timers;
using Windows.Graphics;

namespace CRSim.Views
{
    // �����¼�ί������
    public delegate void SplashScreenClosedEventHandler(object sender, EventArgs e);

    public sealed partial class StartWindow : Window
    {
        // �����¼�
        public event SplashScreenClosedEventHandler SplashScreenClosed;

        public StartWindow()
        {
            this.InitializeComponent();

            AppWindow.Resize(new Windows.Graphics.SizeInt32(500, 180));
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

        }

        // ��ɳ�ʼ�����������������ر�
        public void CompleteInitialization()
        {
            SplashScreenClosed?.Invoke(this, EventArgs.Empty);
            this.Close();
        }
    }
}
