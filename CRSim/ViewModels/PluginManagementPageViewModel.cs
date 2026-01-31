using CRSim.Core.Models.Plugin;
using System.Diagnostics;

namespace CRSim.ViewModels;

public partial class PluginManagementPageViewModel : ObservableObject
{
    [ObservableProperty]
    public partial string PageTitle { get; set; } = "插件管理";

    [ObservableProperty]
    public partial string SearchText { get; set; } = "";

    [ObservableProperty]
    public partial PluginInfo? SelectedPlugin { get; set; }

    public List<PluginInfo> Plugins = IPluginService.OnlinePlugins;

    public List<PluginInfo> FilteredPlugins => [.. Plugins.Where(x => x.Manifest.Name.Contains(SearchText) && (SelectedAuthor == "全体作者" || x.Manifest.Author == SelectedAuthor))];

    public List<string> Authors => ["全体作者",.. Plugins.Select(x => x.Manifest.Author).Distinct()];

    [ObservableProperty]
    public partial string SelectedAuthor { get; set; } = "全体作者";

    private readonly IPluginService _pluginService;
    private readonly IDialogService _dialogService;

    public PluginManagementPageViewModel(IPluginService pluginService,IDialogService dialogService)
    {
        _pluginService = pluginService;
        _dialogService = dialogService;
        _ = InitAsync();
    }
    private async Task InitAsync()
    {
        await _pluginService.LoadOnlinePluginsAsync();
        OnPropertyChanged(nameof(FilteredPlugins));
    }

    [RelayCommand]
    public void PluginSelected(object args)
    {
        if(args is SelectionChangedEventArgs selectionChangedEventArgs && selectionChangedEventArgs.AddedItems.Count == 1)
        {
            SelectedPlugin = (PluginInfo)selectionChangedEventArgs.AddedItems.FirstOrDefault();
        }
    }

    [RelayCommand]
    public async Task InstallPluginOnline()
    {
        await _pluginService.InstallPluginOnlineAsync(SelectedPlugin);
        OnPropertyChanged(nameof(SelectedPlugin));
    }
    [RelayCommand]
    public async Task InstallPluginLocal()
    {
        if (_dialogService.GetFile([".crsp"]) is string filePath)
        {
            try
            {
                await _pluginService.InstallPluginLocalAsync(filePath);
            }
            catch (Exception ex)
            {
                await _dialogService.ShowTextAsync("安装失败", $"{ex}");
            }
        }
    }
    [RelayCommand]
    public void UninstallPlugin()
    {
        if (SelectedPlugin == null) return;
        SelectedPlugin.IsUninstalling = true;
        OnPropertyChanged(nameof(SelectedPlugin));
    }

    [RelayCommand]
    public void CancelUninstallPlugin()
    {
        if (SelectedPlugin == null) return;
        SelectedPlugin.IsUninstalling = false;
        OnPropertyChanged(nameof(SelectedPlugin));
    }
    [RelayCommand]
    public async Task PackPlugin()
    {
        if (SelectedPlugin == null) return;
        string? filePath = _dialogService.SaveFile(".crsp", $"{SelectedPlugin.Manifest.Id}");
        if (filePath == null) return;
        try
        {
            await _pluginService.PackPluginAsync(SelectedPlugin, filePath);
        }
        catch (Exception ex)
        {
            await _dialogService.ShowTextAsync("打包失败", $"{ex}");
        }
    }
    [RelayCommand]
    public static void Restart()
    {
        string exePath = Environment.ProcessPath;
        Process.Start(new ProcessStartInfo("cmd", $"/C timeout /t 0.75 & start \"\" \"{exePath}\"")
        {
            CreateNoWindow = true,
            UseShellExecute = false
        });
        Application.Current.Exit();
    }

    [RelayCommand]
    public void SourceSelected(object args)
    {
        if(args is SelectorBarItem selectorBarItem)
        {
            SelectedPlugin = null;
            Plugins = (selectorBarItem?.Text == "本地") ? IPluginService.OnlinePlugins : IPluginService.LoadedPlugins;
            OnPropertyChanged(nameof(Authors));
            SelectedAuthor = "全体作者";
            OnPropertyChanged(nameof(FilteredPlugins));
        }
    }

    [RelayCommand]
    public static void OpenPluginFolder()
    {
        Process.Start(new ProcessStartInfo
        {
            FileName = AppPaths.PluginsRootPath,
            UseShellExecute = true
        });
    }

    [RelayCommand]
    public async Task Refresh()
    {
        await _pluginService.LoadOnlinePluginsAsync();
        OnPropertyChanged(nameof(Authors));
        SelectedAuthor = "全体作者";
        OnPropertyChanged(nameof(FilteredPlugins));
    }

    [RelayCommand]
    public void Search(object args)
    {
        OnPropertyChanged(nameof(FilteredPlugins));
    }
}