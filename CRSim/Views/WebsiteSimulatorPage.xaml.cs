namespace CRSim.Views;

public sealed partial class WebsiteSimulatorPage : Page
{
    public WebsiteSimulatorPageViewModel ViewModel { get; } = App.AppHost.Services.GetService<WebsiteSimulatorPageViewModel>();

    public WebsiteSimulatorPage()
    {
        InitializeComponent();
        DataContext = ViewModel;
    }
}
