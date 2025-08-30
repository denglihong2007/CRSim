namespace CRSim.Views;

public sealed partial class DashboardPage : Page
{
    public DashboardPageViewModel ViewModel { get; } = App.AppHost.Services.GetService<DashboardPageViewModel>();

    public DashboardPage()
    {
        InitializeComponent();
        DataContext = ViewModel;
    }
}
