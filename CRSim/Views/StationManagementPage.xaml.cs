namespace CRSim.Views;

public sealed partial class StationManagementPage : Page
{
    public StationManagementPageViewModel ViewModel { get; } = App.AppHost.Services.GetService<StationManagementPageViewModel>();

    public StationManagementPage()
    {
        InitializeComponent();
        DataContext = ViewModel;
    }

    private void Page_Loaded(object sender, RoutedEventArgs e)
    {
        App.AppHost.Services.GetService<IDialogService>().XamlRoot = this.XamlRoot;
    }
}
