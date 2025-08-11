using CRSim.Core.Abstractions;
using CRSim.Core.Models;
using System.Windows.Threading;

namespace CRSim.ScreenSimulator.Abstractions
{
    public interface IScreenViewModel
    {
        ITimeService TimeService { get; }
        Dispatcher UIDispatcher { get; set; }
        void LoadData(Station station, TicketCheck? ticketCheck, string platform);
        string? Text { get; set; }
        Uri Video { get; set; }
        int Location { get; set; }
    }
}
