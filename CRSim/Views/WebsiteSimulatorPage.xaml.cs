namespace CRSim.Views;

public sealed partial class WebsiteSimulatorPage : Page
{
    public WebsiteSimulatorPageViewModel ViewModel { get; } = App.GetService<WebsiteSimulatorPageViewModel>();

    public WebsiteSimulatorPage()
    {
        InitializeComponent();
        DataContext = ViewModel;
    }
}
