namespace CRSim.Core.Models
{
    public class TrainNumber
    {
        public string Number { get; set; }
        public List<TrainStop> TimeTable { get; set; } = [];
        public List<Section> Sections { get; set; } = [];
    }
}
