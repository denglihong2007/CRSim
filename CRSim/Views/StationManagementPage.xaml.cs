namespace CRSim.Views;

public sealed partial class StationManagementPage : Page
{
    public StationManagementPageViewModel ViewModel { get; } = App.GetService<StationManagementPageViewModel>();

    public StationManagementPage()
    {
        InitializeComponent();
        DataContext = ViewModel;
    }

    private void Page_Loaded(object sender, RoutedEventArgs e)
    {
        App.GetService<IDialogService>().XamlRoot = this.XamlRoot;
    }
}
