using System.Collections.ObjectModel;

namespace CRSim.Core.Models
{
    public class WaitingArea
    {
        public string Name { get; set; }
        public ObservableCollection<TicketCheck> TicketChecks { get; set; } = [];
    }
}
