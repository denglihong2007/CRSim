namespace CRSim.Views;

public sealed partial class TrainNumberManagementPage : Page
{
    public TrainNumberManagementPageViewModel ViewModel { get; } = App.GetService<TrainNumberManagementPageViewModel>();

    public TrainNumberManagementPage()
    {
        InitializeComponent();
        DataContext = ViewModel;
    }

    private void Page_Loaded(object sender, RoutedEventArgs e)
    {
        App.GetService<IDialogService>().XamlRoot = this.XamlRoot;
    }
}
