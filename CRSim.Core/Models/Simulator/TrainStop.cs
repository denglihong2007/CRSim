using CRSim.Core.Converters;
using System.Text.Json.Serialization;

namespace CRSim.Core.Models
{
    public class TrainStop
    {
        public string? Number { get; set; }

        public string? Station { get; set; }

        [JsonConverter(typeof(TimeSpanJsonConverter))]
        public TimeSpan? ArrivalTime { get; set; }

        [JsonConverter(typeof(TimeSpanJsonConverter))]
        public TimeSpan? DepartureTime { get; set; }

        public List<Guid>? TicketCheckIds { get; set; }

        public string? Origin { get; set; }

        public string? Terminal { get; set; }

        public string? Platform { get; set; }

        public string? Landmark { get; set; }

        public int Length { get; set; }

        [JsonIgnore]
        public StationType StationType
        {
            get
            {
                if (ArrivalTime == null)
                {
                    return StationType.Departure;
                }
                else if (DepartureTime == null)
                {
                    return StationType.Arrival;
                }
                return StationType.Both;
            }
        }
    }
}
