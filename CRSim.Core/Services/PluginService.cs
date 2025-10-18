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

public class PluginService(INetworkService networkService, ISettingsService settingsService) : IPluginService
{
    public static readonly string PluginManifestFileName = "manifest.json";
    public static readonly string StyleInfoFileName = "style.json";

    private string IndexUrl => $"{_settings.Api.BaseApi}/GetFile?fileName=plugins.json";

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
                StyleInfo = manifest.Type == "ScreenStyle" ? JsonSerializer.Deserialize(File.ReadAllText(Path.Combine(Path.GetFullPath(pluginDir), StyleInfoFileName)),JsonContextWithCamelCase.Default.StyleInfo) : null,
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
    private readonly Settings _settings = settingsService.GetSettings();

    public async Task LoadOnlinePluginsAsync()
    {
        var pluginManifests = await networkService.GetOnlinePluginsAsync(IndexUrl);
        IPluginService.OnlinePluginsInternal.Clear();
        foreach (var Manifest in pluginManifests ?? [])
        {
            var localInfo = IPluginService.LoadedPluginsInternal.FirstOrDefault(x => x.Manifest.Id == Manifest.Id);
            var info = new PluginInfo
            {
                Manifest = Manifest,
                RealIconPath = $"{_settings.Api.BaseApi}/GetFile?fileName=icons/{Manifest.Id}.png",
                LoadStatus = localInfo?.LoadStatus ?? PluginLoadStatus.NotLoaded,
                PluginFolderPath = localInfo?.PluginFolderPath ?? string.Empty,
            };
            IPluginService.OnlinePluginsInternal.Add(info);
        }
        await Task.CompletedTask;
    }

    public async Task InstallPluginOnlineAsync(PluginInfo plugin)
    {
        var id = plugin.Manifest.Id;
        var packageUrl = $"{_settings.Api.BaseApi}/GetFile?fileName=plugins/{id}{IPluginService.PluginPackageExtension}";
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

    public async Task InstallPluginLocalAsync(string filePath)
    {
        var tempDir = Path.Combine(AppPaths.TempPath, "Plugins", Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);
        System.IO.Compression.ZipFile.ExtractToDirectory(filePath, tempDir);

        var manifestPath = Path.Combine(tempDir, PluginManifestFileName);
        if (!File.Exists(manifestPath))
        {
            throw new FileNotFoundException("�������ȱ�� manifest.json �ļ���");
        }
        var manifestYaml = File.ReadAllText(manifestPath);
        if( JsonSerializer.Deserialize(manifestYaml, JsonContextWithCamelCase.Default.PluginManifest) is not PluginManifest manifest || manifest.Id == "")
        {
            throw new InvalidDataException("������е� manifest.json �ļ���ʽ����ȷ��");
        }

        // ��ѹ�����Ŀ¼
        var destDir = Path.Combine(AppPaths.PluginsRootPath, manifest.Id);
        if (Directory.Exists(destDir))
            Directory.Delete(destDir, true);
        System.IO.Compression.ZipFile.ExtractToDirectory(filePath, destDir);

        var info = new PluginInfo
        {
            Manifest = manifest,
            PluginFolderPath = destDir,
            RealIconPath = Path.Combine(destDir, "icon.png"),
            StyleInfo = manifest.Type == "ScreenStyle" ? JsonSerializer.Deserialize(File.ReadAllText(Path.Combine(destDir, StyleInfoFileName)), JsonContextWithCamelCase.Default.StyleInfo) : null,
            RestartRequired = true
        };
        // ������ʱ�ļ�
        try { Directory.Delete(tempDir, true); } catch { }
        IPluginService.LoadedPluginsInternal.Add(info);
        await Task.CompletedTask;
    }

    public async Task PackPluginAsync(PluginInfo plugin, string filePath)
    {
        var pluginDir = plugin.PluginFolderPath;
        if (string.IsNullOrWhiteSpace(pluginDir) || !Directory.Exists(pluginDir))
            throw new DirectoryNotFoundException("���Ŀ¼�����ڡ�");

        // ��ʱĿ¼���ڴ��
        var tempDir = Path.Combine(AppPaths.TempPath, "Pack", Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);

        // ���������ļ�����ʱĿ¼���ų�ָ��dll
        foreach (var file in Directory.EnumerateFiles(pluginDir, "*", SearchOption.AllDirectories))
        {
            var fileName = Path.GetFileName(file);
            if (fileName.Equals("Microsoft.Windows.SDK.NET.dll", StringComparison.OrdinalIgnoreCase) ||
                fileName.Equals("WinRT.Runtime.dll", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }
            var relativePath = Path.GetRelativePath(pluginDir, file);
            var destPath = Path.Combine(tempDir, relativePath);
            Directory.CreateDirectory(Path.GetDirectoryName(destPath)!);
            File.Copy(file, destPath, true);
        }

        // ���Ŀ���ļ��Ѵ��ڣ���ɾ��
        if (File.Exists(filePath))
            File.Delete(filePath);

        // ���Ϊzip
        System.IO.Compression.ZipFile.CreateFromDirectory(tempDir, filePath);

        // ������ʱĿ¼
        try { Directory.Delete(tempDir, true); } catch { }

        await Task.CompletedTask;
    }
}