namespace CRSim.Views;

public sealed partial class TrainNumberManagementPage : Page
{
    public TrainNumberManagementPageViewModel ViewModel { get; } = App.AppHost.Services.GetService<TrainNumberManagementPageViewModel>();

    public TrainNumberManagementPage()
    {
        InitializeComponent();
        DataContext = ViewModel;
    }

    private void Page_Loaded(object sender, RoutedEventArgs e)
    {
        App.AppHost.Services.GetService<IDialogService>().XamlRoot = this.XamlRoot;
    }
}
