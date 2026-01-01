namespace CRSim.Core.Models
{
    public class Station
    {
        public string Name { get; set; }

        public List<WaitingArea> WaitingAreas { get; set; } = [];
        
        public List<TrainStop> TrainStops { get; set; } = [];

        public List<Platform> Platforms { get; set; } = [];
    }
}
