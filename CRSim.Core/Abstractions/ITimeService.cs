namespace CRSim.Core.Abstractions
{
    public interface ITimeService
    {
        event EventHandler OneSecondElapsed;

        event EventHandler RefreshSecondsElapsed;

        void Start();

        DateTime GetDateTimeNow();

        DateTime SimulateTime { get; set; }      // 初始模拟时间

        double Speed { get; set; }               // 模拟倍速
    }
}