using System.Diagnostics;

namespace CRSim.Views;

public sealed partial class PluginManagementPage : Page
{
    public PluginManagementPageViewModel ViewModel { get; } = App.GetService<PluginManagementPageViewModel>();

    public PluginManagementPage()
    {
        InitializeComponent();
        DataContext = ViewModel;
    }

    private void SelectorBar_SelectionChanged(SelectorBar sender, SelectorBarSelectionChangedEventArgs args)
    {
    }

    private void Page_Loaded(object sender, RoutedEventArgs e)
    {
        App.GetService<IDialogService>().XamlRoot = this.XamlRoot;
    }
}
