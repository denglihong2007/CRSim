using CRSim.ScreenSimulator.Views;
using System.Windows.Threading;

﻿namespace CRSim
{
    public partial class App : Application
    {
        public IHost AppHost { get; private set; }

        public static MainWindow MainWindow;
        public static string AppVersion { get; set; }

        private Mutex mutex;

        public App()
        {
            InitializeComponent();
            this.DispatcherUnhandledException += App_DispatcherUnhandledException;
        }

        public static T GetService<T>()
    where T : class
        {
            if ((Current as App)!.AppHost.Services.GetService(typeof(T)) is not T service)
            {
                throw new ArgumentException($"{typeof(T)} needs to be registered in ConfigureServices within App.xaml.cs.");
            }

            return service;
        }

        protected override async void OnLaunched(LaunchActivatedEventArgs args)
        {
            AppVersion = Assembly.GetAssembly(typeof(App)).GetName().Version.ToString();
            mutex = new Mutex(true, "CRSim", out bool isNewInstance);
            if (!isNewInstance)
            {
                Environment.Exit(0);
            }
            var parsedOptions = CommandLineParser.Parse(Environment.GetCommandLineArgs());
            if (parsedOptions.Debug)
            {
                LaunchDebugConsole();
            }
            if (!Directory.Exists(AppPaths.AppDataPath))
            {
                Directory.CreateDirectory(AppPaths.AppDataPath);
            }
            AppHost = Host.CreateDefaultBuilder()
                .ConfigureServices((context, services) =>
                {
                    services.AddSingleton(parsedOptions);
                    services.AddSingleton<IPluginService, PluginService>();
                    services.AddSingleton<ISettingsService, SettingsService>();
                    services.AddSingleton<IDatabaseService, DatabaseService>();
                    services.AddSingleton<INetworkService, NetworkService>();
                    services.AddSingleton<IDialogService, DialogService>();
                    services.AddTransient<ITimeService, TimeService>();
                    services.AddSingleton<StyleManager>();
                    services.AddSingleton<MainWindow>();
                    services.AddSingleton<StartWindow>();
                    services.AddTransient<Simulator>();
                    services.AddTransient<DashboardPageViewModel>();
                    services.AddTransient<StationManagementPageViewModel>();
                    services.AddTransient<TrainNumberManagementPageViewModel>();
                    services.AddTransient<WebsiteSimulatorPageViewModel>();
                    services.AddTransient<ScreenSimulatorPageViewModel>();
                    services.AddTransient<PluginManagementPageViewModel>();
                    services.AddTransient<PlatformDiagramPageViewModel>();
                    services.AddTransient<SettingsPageViewModel>();
                    PluginService.InitializePlugins(context, services, parsedOptions.ExternalPluginPath);
                })
            .Build();
            var splashScreenWindow = AppHost.Services.GetRequiredService<StartWindow>();
            splashScreenWindow.Activate();
            await PerformInitializationAsync(splashScreenWindow);
            MainWindow = AppHost.Services.GetRequiredService<MainWindow>();
            MainWindow.Activate();
            splashScreenWindow.Close();
        }

        private static async Task PerformInitializationAsync(StartWindow startWindow)
        { 
            try
            {
                if(!Debugger.IsAttached) await Task.Delay(2000);
                startWindow.Status = "正在加载设置...";
                GetService<ISettingsService>().LoadSettings();
                if (!Debugger.IsAttached) await Task.Delay(500);
                startWindow.Status = "正在加载数据库...";
                GetService<IDatabaseService>().ImportData(AppPaths.ConfigFilePath);
                if (!Debugger.IsAttached) await Task.Delay(500);
                startWindow.Status = new[] { "正在控票…", "正在关闭垃圾桶盖…", "正在刷绿车底…", "正在准备降弓用刑…" }[new Random().Next(4)];
                if (!Debugger.IsAttached) await Task.Delay(500);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"初始化错误: {ex.Message}");
            }
        }

        [System.Runtime.InteropServices.DllImport("kernel32.dll")]
        private static extern bool AllocConsole();

        public static void LaunchDebugConsole()
        {
            AllocConsole();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine(@"
        ___ _____________________
   _.-''---'  |  ___ ___ ___ ___|
  (       __|_|_[___[___[___[__]  
 =-(_)--(_)--'      (O)   (O) ");
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine("    CRSim - 国铁信息显示模拟 v" + AppVersion);
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("Debug console started.\n");
            Console.ForegroundColor = ConsoleColor.White;
        }

        private void App_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            var dialog = new ErrorDialog(e.Exception.ToString());
            dialog.ShowDialog();
            e.Handled = true;
        }
    }
}
