namespace CRSim.Views;

public sealed partial class ScreenSimulatorPage : Page
{
    public ScreenSimulatorPageViewModel ViewModel { get; } = App.AppHost.Services.GetService<ScreenSimulatorPageViewModel>();

    public ScreenSimulatorPage()
    {
        InitializeComponent();
        DataContext = ViewModel;
    }
}
