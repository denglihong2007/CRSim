using CRSim.Core.Models.Plugin;
using System.Text.Json.Serialization;

namespace CRSim.Core.Models
{
    [JsonSerializable(typeof(Json))]
    [JsonSerializable(typeof(UpdateInfo))]
    [JsonSourceGenerationOptions(WriteIndented = true, DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
    public partial class JsonContext : JsonSerializerContext
    {
    }

    [JsonSerializable(typeof(List<PluginManifest>))]
    [JsonSerializable(typeof(StyleInfo))]
    [JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
    public partial class JsonContextWithCamelCase : JsonSerializerContext
    {
    }
}

