namespace CRSim.Core.Abstractions
{
    /// <summary>
    /// 定义 API 客户端工厂的合约，用于创建 IApi 实例。
    /// </summary>
    public interface IApiFactory
    {
        /// <summary>
        /// 根据客户端类型名称和基础 URL 创建 IApi 实例。
        /// </summary>
        /// <param name="clientName">客户端的唯一名称。</param>
        /// <returns>返回具体的 IApi 实现。</returns>
        IApi CreateApi(string clientName);
    }
}
