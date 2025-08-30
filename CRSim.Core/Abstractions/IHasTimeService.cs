namespace CRSim.Core.Abstractions
{
    public interface IHasTimeService
    {
        ITimeService TimeService { get; set; }
    }
}
