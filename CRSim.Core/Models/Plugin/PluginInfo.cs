using CommunityToolkit.Mvvm.ComponentModel;
using CRSim.Core.Abstractions;
using CRSim.Core.Enums;

namespace CRSim.Core.Models.Plugin;
public partial class PluginInfo : ObservableRecipient
{
    [ObservableProperty]
    private int _downloadProgress = 0;

    public bool IsAvailableOnMarket => IPluginService.OnlinePlugins.Any(x => x.Manifest.Id == Manifest.Id);

    public bool IsUpdateAvailable
    {
        get
        {
            if (IsAvailableOnMarket)
            {
                if(IPluginService.OnlinePlugins.Where(x => x.Manifest.Id == Manifest.Id).FirstOrDefault() is PluginInfo onlinePlugin)
                {
                    return onlinePlugin.Manifest.Version != Manifest.Version;
                }
            }
            return false;
        }
    }

    [ObservableProperty]
    private PluginManifest _manifest = new();

    [ObservableProperty]
    private bool _restartRequired = false;

    public PluginLoadStatus LoadStatus { get; internal set; } = PluginLoadStatus.NotLoaded;

    public bool IsEnabled
    {
        get
        {
            return !File.Exists(Path.Combine(PluginFolderPath, ".disabled"));
        }
        set
        {
            var path = Path.Combine(PluginFolderPath, ".disabled");
            RestartRequired = true;
            if (value)
            {
                File.Delete(path);
            }
            else
            {
                File.WriteAllText(path, "");
            }
            OnPropertyChanged();
        }
    }

    public string PluginFolderPath { get; internal set; } = "";
    public string RealIconPath { get; set; } = "";
    public bool IsUninstalling
    {
        get
        {
            return File.Exists(Path.Combine(PluginFolderPath, ".uninstall"));
        }
        set
        {
            var path = Path.Combine(PluginFolderPath, ".uninstall");
            if (value)
            {
                RestartRequired = true;
                File.WriteAllText(path, "");
            }
            else
            {
                RestartRequired = false;
                File.Delete(path);
            }
            OnPropertyChanged();
        }
    }
    public Exception? Exception { get; internal set; }

    [ObservableProperty]
    private StyleInfo? styleInfo;

    [ObservableProperty]
    private Json? data;
}