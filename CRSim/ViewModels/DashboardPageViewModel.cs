using System.Windows;

namespace CRSim.ViewModels;
public partial class DashboardPageViewModel : ObservableObject
{
    public StyleManager StyleManager { get; }
    public DashboardPageViewModel(StyleManager styleManager)
    {
        StyleManager = styleManager;
    }
}