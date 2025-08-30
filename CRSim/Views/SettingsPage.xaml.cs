using System.Diagnostics;

namespace CRSim.Views;

public sealed partial class SettingsPage : Page
{
    public SettingsPageViewModel ViewModel { get; } = App.AppHost.Services.GetService<SettingsPageViewModel>();

    public SettingsPage()
    {
        InitializeComponent();
        DataContext = ViewModel;
    }
    private void Open_Issues(object sender, RoutedEventArgs e)
    {
        Process.Start(new ProcessStartInfo("https://github.com/denglihong2007/CRSim/issues/new") { UseShellExecute = true });
    }

    private void Page_Loaded(object sender, RoutedEventArgs e)
    {
        App.AppHost.Services.GetService<IDialogService>().XamlRoot = this.XamlRoot;
    }
}
