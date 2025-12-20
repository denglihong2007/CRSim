using System.Runtime.InteropServices;
using Windows.ApplicationModel.DataTransfer;

namespace CRSim.Views
{
    public partial class ErrorDialog : Window
    {
        private const string IssuesUrlBase = "https://github.com/denglihong2007/CRSim/issues/new";
        public ErrorDialog(string message)
        {
            InitializeComponent();
            SystemBackdrop = new Microsoft.UI.Xaml.Media.MicaBackdrop();
            ErrorMessage.Text = message ?? string.Empty;
        }
        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void CopyButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var package = new DataPackage();
                package.SetText(ErrorMessage.Text ?? string.Empty);
                Clipboard.SetContent(package);
            }
            catch (Exception)
            {
                // 复制失败时静默处理，避免抛出未处理异常
            }
        }

        private void ReportButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var title = Uri.EscapeDataString("CRSim错误报告");
                string deviceInfo = $"{RuntimeInformation.OSDescription}";
                string framework = RuntimeInformation.FrameworkDescription;
                var bodyText = $"""
                CRSim版本：{App.AppVersion}
                设备信息：{deviceInfo} (Runtime: {framework})
                请描述复现步骤：

                ----------------------
                错误信息：{ErrorMessage.Text}
                """;
                var body = Uri.EscapeDataString(bodyText); 
                var url = $"{IssuesUrlBase}?title={title}&labels=Bug&body={body}";

                var psi = new ProcessStartInfo(url)
                {
                    UseShellExecute = true
                };
                Process.Start(psi);
            }
            catch (Exception)
            {
                // 打开浏览器失败时静默处理
            }
        }
    }
}
