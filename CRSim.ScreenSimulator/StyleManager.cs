using CRSim.Core.Abstractions;
using CRSim.Core.Enums;
using CRSim.Core.Models.Plugin;
using CRSim.ScreenSimulator.Abstractions;
using CRSim.ScreenSimulator.Models;
using CRSim.ScreenSimulator.Views;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Windows.Controls;
using Windows.System;

namespace CRSim.ScreenSimulator
{
	public class StyleManager
	{
        public List<PluginInfo> StyleInfos => [.. IPluginService.LoadedPlugins.Where(x => x.Manifest.Type == "ScreenStyle" && x.LoadStatus == PluginLoadStatus.Loaded)];

        public static IServiceProvider ServiceProvider { get; set; }

        public ObservableCollection<SimulatorWindow> ActiveWindows { get; set; } = [];

        public DispatcherQueue DispatcherQueue { get; set; } = DispatcherQueue.GetForCurrentThread();

        public StyleManager(IServiceProvider serviceProvider)
		{
            ServiceProvider = serviceProvider;
        }
        public void ShowWindow(Type page, Session session)
        {
            Thread thread = new(() =>
            {
                Page viewInstance = (Page)ServiceProvider.GetService(page);
                if (viewInstance.DataContext is IScreenViewModel viewModel)
                {
                    var timeService = viewModel.TimeService;
                    timeService.SimulateTime = session.SimulateTime;
                    timeService.Start();

                    viewModel.UIDispatcher = System.Windows.Threading.Dispatcher.CurrentDispatcher;
                    if (session.Text != null)
                    {
                        viewModel.Text = session.Text;
                    }
                    if (session.Loaction != null && session.Loaction != 0)
                    {
                        viewModel.Location = session.Loaction.Value;
                    }
                    if (session.Video != null)
                    {
                        viewModel.Video = session.Video;
                    }
                    viewModel.LoadData(session.Station, session.TicketCheck, session.PlatformName);
                    var win = new SimulatorWindow(viewInstance, timeService, this, session);
                    DispatcherQueue.TryEnqueue(() =>
                    {
                        ActiveWindows.Add(win);
                    });
                    win.Show();
                    System.Windows.Threading.Dispatcher.Run();
                }
            });

            thread.SetApartmentState(ApartmentState.STA); 
            thread.IsBackground = true;
            thread.Start();
        }
    }
}
