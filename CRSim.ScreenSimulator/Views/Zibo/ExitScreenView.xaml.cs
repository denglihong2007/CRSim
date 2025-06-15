using CRSim.ScreenSimulator.ViewModels.Zibo;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace CRSim.ScreenSimulator.Views.Zibo
{
    public partial class ExitScreenView : BaseScreenView
    {
        public ExitScreenViewModel ViewModel { get; }
        public ExitScreenView(ExitScreenViewModel viewModel)
        {
            InitializeComponent();
            ViewModel = viewModel;
            DataContext = viewModel;
        }
    }
}
