using CRSim.Core.Abstractions;
using CRSim.Core.Attributes;
using CRSim.Core.Enums;
using CRSim.Core.Models;
using CRSim.Core.Models.Plugin;
using CRSim.Core.Utils;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Reflection;
using System.Text.Json;
using Windows.System;
using Downloader;

namespace CRSim.Core.Services;

public class PluginService : IPluginService
{
    public static readonly string PluginManifestFileName = "manifest.json";
    public static readonly string StyleInfoFileName = "style.json";

    private string IndexUrl => $"{_settings.ApiUri}/GetFile?fileName=plugins.json";

    public static void InitializePlugins(HostBuilderContext context, IServiceCollection services,string externalPluginPath)
    {
        Console.WriteLine("��ʼ���ز��");
        if (!Directory.Exists(AppPaths.PluginsRootPath))
        {
            Directory.CreateDirectory(AppPaths.PluginsRootPath);
        }

        var pluginDirs = Directory.EnumerateDirectories(AppPaths.PluginsRootPath);
        if(!string.IsNullOrEmpty(externalPluginPath))
        {
            pluginDirs = pluginDirs.Append(externalPluginPath);
        }

        foreach (var pluginDir in pluginDirs)
        {
            Console.WriteLine($"��ǰ�����ѰĿ¼: {pluginDir}");
            if (string.IsNullOrWhiteSpace(pluginDir))
                continue;
            var manifestPath = Path.Combine(pluginDir, PluginManifestFileName);
            if (!File.Exists(manifestPath))
            {
                continue;
            }

            var manifestYaml = File.ReadAllText(manifestPath);
            PluginManifest manifest = new();
            try
            {
                manifest = JsonSerializer.Deserialize(manifestYaml,JsonContextWithCamelCase.Default.PluginManifest);
                Console.WriteLine("�ɹ�����JSON�ļ�: " + manifestPath);
            }
            catch (Exception ex)
            {
                Console.WriteLine("JSON����ʧ��");
                Console.WriteLine(ex.ToString());
                continue;
            }
            Console.WriteLine("�ҵ����: " + manifest.Id);
            var info = new PluginInfo
            {
                Manifest = manifest,
                PluginFolderPath = Path.GetFullPath(pluginDir),
                RealIconPath = Path.Combine(Path.GetFullPath(pluginDir), "icon.png"),
                StyleInfo = JsonSerializer.Deserialize(File.ReadAllText(Path.Combine(Path.GetFullPath(pluginDir), StyleInfoFileName)),JsonContextWithCamelCase.Default.StyleInfo),
            };
            if (info.IsUninstalling)
            {
                Directory.Delete(pluginDir, true);
                continue;
            }
            if (!info.IsEnabled)
            {
                info.LoadStatus = PluginLoadStatus.Disabled;
            }
            IPluginService.LoadedPluginsInternal.Add(info);
        }

        foreach (var info in IPluginService.LoadedPluginsInternal)
        {
            if(info.LoadStatus == PluginLoadStatus.Disabled) 
            {
                continue;
            }
            var manifest = info.Manifest;
            var pluginDir = info.PluginFolderPath;
            try
            {
                Console.WriteLine("���Լ��ز��: " + manifest.Id);
                var fullPath = Path.GetFullPath(Path.Combine(pluginDir, manifest.EntranceAssembly));
                var loadContext = new PluginLoadContext(fullPath);
                var assembly = loadContext.LoadFromAssemblyPath(fullPath);
                var entrance = assembly.ExportedTypes.FirstOrDefault(x =>
                    x.BaseType == typeof(PluginBase) ||
                    x.GetCustomAttributes().FirstOrDefault(a => a.GetType() == typeof(PluginEntrance)) != null);

                if (entrance == null)
                {
                    continue;
                }

                if (Activator.CreateInstance(entrance) is not PluginBase entranceObj)
                {
                    continue;
                }

                entranceObj.Info = info;
                entranceObj.Initialize(context, services);
                services.AddSingleton(typeof(PluginBase), entranceObj);
                services.AddSingleton(entrance, entranceObj);
                info.LoadStatus = PluginLoadStatus.Loaded;
                Console.WriteLine($"���ز���ɹ�: {pluginDir} ({manifest.Version})");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"���ز��ʧ��: {ex}");
                info.Exception = ex;
                info.LoadStatus = PluginLoadStatus.Error;
            }
        }        
    }
    private readonly Models.Settings _settings;
    private readonly INetworkService _networkService;
    public PluginService(INetworkService networkService,ISettingsService settingsService)
    {
        _networkService = networkService;
        _settings = settingsService.GetSettings();
        LoadOnlinePlugins();
    }
    public void LoadOnlinePlugins()
    {
        var pluginManifests = _networkService.GetOnlinePlugins(IndexUrl);
        IPluginService.OnlinePluginsInternal.Clear();
        foreach (var Manifest in pluginManifests ?? [])
        {
            var localInfo = IPluginService.LoadedPluginsInternal.FirstOrDefault(x => x.Manifest.Id == Manifest.Id);
            var info = new PluginInfo
            {
                Manifest = Manifest,
                RealIconPath = $"{_settings.ApiUri}/GetFile?fileName=icons/{Manifest.Id}.png",
                LoadStatus = localInfo?.LoadStatus ?? PluginLoadStatus.NotLoaded,
                PluginFolderPath = localInfo?.PluginFolderPath ?? string.Empty,
            };
            IPluginService.OnlinePluginsInternal.Add(info);
        }
    }

    public async Task InstallPluginAsync(PluginInfo plugin)
    {
        var id = plugin.Manifest.Id;
        var packageUrl = $"{_settings.ApiUri}/GetFile?fileName=plugins/{id}{IPluginService.PluginPackageExtension}";
        var tempDir = Path.Combine(AppPaths.TempPath, "Plugins", Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);
        var packagePath = Path.Combine(tempDir, $"{id}{IPluginService.PluginPackageExtension}");
        DispatcherQueue dispatcherQueue = DispatcherQueue.GetForCurrentThread();
        var downloader = new DownloadService();
        plugin.DownloadProgress = 0;
        try
        {
            downloader.DownloadProgressChanged += (s, e) =>
            {
                dispatcherQueue.TryEnqueue(() =>
                {
                    plugin.DownloadProgress = (int)e.ProgressPercentage;
                    Console.WriteLine(e.ProgressPercentage);
                });
            };
            await downloader.DownloadFileTaskAsync(packageUrl, packagePath);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            plugin.DownloadProgress = 0;
            return;
        }

        // ��ѹ�����Ŀ¼
        var destDir = Path.Combine(AppPaths.PluginsRootPath, id);
        if (Directory.Exists(destDir))
            Directory.Delete(destDir, true);
        Directory.CreateDirectory(destDir);
        System.IO.Compression.ZipFile.ExtractToDirectory(packagePath, destDir);

        // ������ʱ�ļ�
        try { Directory.Delete(tempDir, true); } catch { }

        plugin.DownloadProgress = 0;
        plugin.RestartRequired = true;
    }
}