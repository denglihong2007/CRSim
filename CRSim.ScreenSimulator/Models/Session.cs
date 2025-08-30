using CRSim.Core.Models;

namespace CRSim.ScreenSimulator.Models
{
    public class Session
    {
        public required Guid ID { get; set; }
        public required string StyleName { get; set; }
        public required DateTime SimulateTime { get; set; }
        public string? Text { get; set; }
        public Uri? Video { get; set; }
        public Station? Station { get; set; }
        public TicketCheck? TicketCheck { get; set; }
        public string? PlatformName { get; set; }
        public int? Loaction { get; set; }
    }
}
