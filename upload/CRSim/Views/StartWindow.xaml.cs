using System.Timers;

namespace CRSim.Views
{
    // �����¼�ί������
    public delegate void SplashScreenClosedEventHandler(object sender, EventArgs e);

    public sealed partial class StartWindow : Window
    {
        // �����¼�
        public event SplashScreenClosedEventHandler SplashScreenClosed;

        // ��������ر���
        private readonly Timer _progressTimer;
        private double _currentProgress = 0;
        private const double ProgressIncrement = 1; // ÿ�����ӵĽ���ֵ
        private const int TimerInterval = 50; // ��ʱ�����(����)

        public StartWindow()
        {
            this.InitializeComponent();

            // ��ʼ����ʱ��
            _progressTimer = new Timer(TimerInterval);
            _progressTimer.Elapsed += OnProgressTimerElapsed;
            _progressTimer.Start();
        }

        // ��ʱ���¼��������½�����
        private void OnProgressTimerElapsed(object sender, ElapsedEventArgs e)
        {
            // ȷ����UI�̸߳���
            this.DispatcherQueue.TryEnqueue(() =>
            {
                // ���ӽ��ȣ����90%����10%�������ɣ�
                _currentProgress += ProgressIncrement;
                if (_currentProgress > 90)
                {
                    _currentProgress = 90;
                }

                progressBar.Value = _currentProgress;
            });
        }

        // ��ɳ�ʼ�����������������ر�
        public void CompleteInitialization()
        {
            // ֹͣ��ʱ��
            _progressTimer.Stop();
            _progressTimer.Dispose();

            // ���ٽ�����������
            this.DispatcherQueue.TryEnqueue(() =>
            {
                progressBar.Value = 100;
            });

            // �����ӳٺ�رգ����û������������
            Task.Delay(300).ContinueWith(_ =>
            {
                this.DispatcherQueue.TryEnqueue(() =>
                {
                    SplashScreenClosed?.Invoke(this, EventArgs.Empty);
                    this.Close();
                });
            });
        }
    }
}
