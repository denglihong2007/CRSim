using System.Windows;

namespace CRSim.ScreenSimulator.Views
{
    public partial class ErrorDialog : Window
    {
        public ErrorDialog(string message)
        {
            InitializeComponent();
            ErrorMessage.Text = message;
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
