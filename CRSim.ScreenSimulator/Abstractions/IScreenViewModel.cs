using CRSim.Core.Abstractions;
using CRSim.Core.Services;
using System.Windows.Threading;

namespace CRSim.ScreenSimulator.Abstractions
{
    public interface IScreenViewModel
    {
        public TimeService TimeService { get; }
        public Dispatcher UIDispatcher { get; set; }
        public string Text { get; set; }
        public int Location { get; set; }
        public string Video { get; set; }

        public void LoadData(string station, string ticketCheck, string platformName);
    }
}
