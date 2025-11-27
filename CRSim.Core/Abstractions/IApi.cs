namespace CRSim.Core.Abstractions
{
    public interface IApi
    {
        string Name { get; }
        string BaseApi { get; }
        string UpdateApi { get; }
    }
}
