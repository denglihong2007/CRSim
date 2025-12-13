namespace CRSim.Views;

public sealed partial class PlatformDiagramPage : Page
{
    public PlatformDiagramPageViewModel ViewModel { get; } = App.GetService<PlatformDiagramPageViewModel>();

    public PlatformDiagramPage()
    {
        InitializeComponent();
        DataContext = ViewModel;
    }

    private void Page_Loaded(object sender, RoutedEventArgs e)
    {
        App.GetService<IDialogService>().XamlRoot = this.XamlRoot;
    }
}
