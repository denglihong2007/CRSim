using CRSim.Core.Models;
using CRSim.Core.Models.Plugin;

namespace CRSim.Core.Abstractions
{
    public interface INetworkService
    {
        Task<List<TrainStop>?> GetTimeTableAsync(string number);
        Task<List<TrainStop>?> GetTrainNumbersAsync(string name);
        List<PluginManifest>? GetOnlinePlugins(string uri);
        Task<UpdateInfo?> GetUpdateAsync(string uri);
    }
}
