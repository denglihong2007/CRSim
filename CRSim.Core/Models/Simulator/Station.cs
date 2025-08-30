using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

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
