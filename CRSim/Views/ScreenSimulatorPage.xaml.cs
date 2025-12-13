namespace CRSim.Views;

public sealed partial class ScreenSimulatorPage : Page
{
    public ScreenSimulatorPageViewModel ViewModel { get; } = App.GetService<ScreenSimulatorPageViewModel>();

    public ScreenSimulatorPage()
    {
        InitializeComponent();
        DataContext = ViewModel;
    }
}
