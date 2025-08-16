using System.Diagnostics;
using System.Threading;

namespace CRSim
{
    public partial class App : Application
    {
        public static IHost AppHost { get; private set; }
        public static MainWindow MainWindow;
        public string[] commandLineArgs;
        public CommandLineOptions parsedOptions;
        public static string AppVersion { get; set; } = Assembly.GetAssembly(typeof(App)).GetName().Version.ToString();
        private readonly Mutex mutex;

        public App()
        {
            mutex = new Mutex(true, "CRSim", out bool isNewInstance);
            if (!isNewInstance)
            {
                Environment.Exit(0);
            }

            var args = Environment.GetCommandLineArgs();
            parsedOptions = CommandLineParser.Parse(args);
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
                    services.AddSingleton<IFlagService, FlagService>();
                    services.AddSingleton<StyleManager>();
                    services.AddSingleton<MainWindow>();
                    services.AddSingleton<StartWindow>();
                    services.AddSingleton<WelcomeWindow>();
                    services.AddTransient<DashboardPageViewModel>();
                    services.AddTransient<StationManagementPageViewModel>();
                    services.AddTransient<ScreenSimulatorPageViewModel>();
                    services.AddTransient<SettingsPageViewModel>();
                    services.AddTransient<PluginManagementPageViewModel>();
                    PluginService.InitializePlugins(context, services, parsedOptions.ExternalPluginPath);
                })
            .Build();
            InitializeComponent();
        }

        protected override async void OnLaunched(LaunchActivatedEventArgs args)
        {

            var splashScreenWindow = AppHost.Services.GetRequiredService<StartWindow>();
            splashScreenWindow.Activate();
            await PerformInitializationAsync(splashScreenWindow);


            var welcomeWindow = AppHost.Services.GetRequiredService<WelcomeWindow>();
            welcomeWindow.Activate();
            splashScreenWindow.Close();
            if (AppHost.Services.GetRequiredService<ISettingsService>().HasShownWelcome == false) await App.AppHost.Services.GetRequiredService<IFlagService>().WaitCloseAsync();

            MainWindow = AppHost.Services.GetRequiredService<MainWindow>();
            MainWindow.Activate();
            welcomeWindow.Close();
        }

        private static async Task PerformInitializationAsync(StartWindow startWindow)
        { 
            try
            {
                if(!Debugger.IsAttached) await Task.Delay(2000);
                startWindow.Status = "正在加载设置...";
                AppHost.Services.GetService<ISettingsService>().LoadSettings();
                if (!Debugger.IsAttached) await Task.Delay(500);
                startWindow.Status = "正在加载数据库...";
                AppHost.Services.GetService<IDatabaseService>().ImportData(AppPaths.ConfigFilePath);
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
    }
}
