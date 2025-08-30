
namespace CRSim
{
public sealed partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            ExtendsContentIntoTitleBar = true;
            SetTitleBar(AppTitleBar);
            SetWindowMinSize(1024, 768);
        }

        private void SetWindowMinSize(int minWidth, int minHeight)
        {
            var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(this);
            var windowId = Microsoft.UI.Win32Interop.GetWindowIdFromWindow(hwnd);
            var appWindow = AppWindow.GetFromWindowId(windowId);
            if (appWindow is not null)
            {
                appWindow.SetPresenter(AppWindowPresenterKind.Overlapped);
                if (appWindow.Presenter is OverlappedPresenter overlappedPresenter)
                {
                    overlappedPresenter.PreferredMinimumWidth = minWidth;
                    overlappedPresenter.PreferredMinimumHeight = minHeight;
                }
            }
        }

        private void nvSample_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs e)
        {
            if(e.IsSettingsSelected)
            {
                contentFrame.Navigate(typeof(Views.SettingsPage));
                return;
            }
            if (nvSample.SelectedItem is NavigationViewItem selectedItem && selectedItem.Tag is string pageTag)
            {
                var pageType = Type.GetType($"CRSim.Views.{pageTag}");
                if (pageType != null)
                {
                    contentFrame.Navigate(pageType);
                }
            }
        }
    }
}
