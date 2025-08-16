using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace CRSim.Views
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    /// 

    public sealed partial class WelcomeWindow: Window
    {
        private readonly ISettingsService _settings;
        private readonly IFlagService _flag;

        public WelcomeWindow()
        {
            InitializeComponent();
            _settings = App.AppHost.Services.GetRequiredService<ISettingsService>();
            _flag = App.AppHost.Services.GetRequiredService<IFlagService>();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            _settings.HasShownWelcome = true;
            _flag.SetCloseFlag();

        }
    }
}
