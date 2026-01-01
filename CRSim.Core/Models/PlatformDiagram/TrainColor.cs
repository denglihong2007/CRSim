using CRSim.Core.Converters;
using System.Drawing;
using System.Text.Json.Serialization;

namespace CRSim.Core.Models.PlatformDiagram
{
    public class TrainColor
    {
        public required string Prefix { get; set; }

        [JsonConverter(typeof(ColorJsonConverter))]
        public required Color Color { get; set; }
    }
}
