using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CRSim.Core.Models
{
    public class Section
    {
        public string From { get; set; }
        public string To { get; set; }
        public Tickets Tickets { get; set; }
    }
}
