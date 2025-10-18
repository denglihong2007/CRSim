using CRSim.Core.Abstractions;

namespace CRSim.Core.Services
{
    /// <summary>
    /// IApiFactory 的具体实现，负责根据配置实例化正确的 IApi 客户端。
    /// </summary>
    public class ApiFactory : IApiFactory
    {
        public IApi CreateApi(string clientName)
        {
            clientName ??= string.Empty;
            return clientName switch
            {
                "镜像站" => new MirrorApi(),
                "官方站" => new OfficialApi(),
                _ => new OfficialApi()
            };
        }
    }
    public class OfficialApi : IApi
    {
        public string Name => "官方站";
        public string BaseApi => "https://47.122.74.193:25565/";
        public string UpdateApi => "https://api.github.com/repos/denglihong2007/CRSim/releases/latest";
    }

    public class MirrorApi : IApi
    {
        public string Name => "镜像站";
        public string BaseApi => "https://crsim.com.cn/api";
        public string UpdateApi => "https://crsim.com.cn/api/version";
    }
}
