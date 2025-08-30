namespace CRSim.Core.Models.Plugin;

/// <summary>
/// 插件元数据
/// </summary>
public class PluginManifest
{
    /// <summary>
    /// 入口程序集。加载插件时，将在此入口程序集中搜索插件类。
    /// </summary>
    /// <example>MyPlugin.dll</example>
    public string EntranceAssembly { get; set; } = "";

    /// <summary>
    /// 插件显示名称。
    /// </summary>
    public string Name { get; set; } = "";

    /// <summary>
    /// 插件ID。
    /// </summary>
    public string Id { get; set; } = "";

    /// <summary>
    /// 插件自述。
    /// </summary>
    public string Description { get; set; } = "";

    /// <summary>
    /// 项目 Url
    /// </summary>
    public string Url { get; set; } = "";

    /// <summary>
    /// 插件版本
    /// </summary>
    public string Version { get; set; } = "";

    /// <summary>
    /// 插件目标 CRSim 版本
    /// </summary>
    public string ApiVersion { get; set; } = "";

    /// <summary>
    /// 插件作者
    /// </summary>
    public string Author { get; set; } = "";

    /// <summary>
    /// 插件类型
    /// </summary>
    public string Type { get; set; } = "";
}