namespace CRSim.Core.Models
{
    public static class TrainStatus
    {
        // 00:00:01 is reserved for "delay unknown"; normal input/export paths only use minute precision.
        public static readonly TimeSpan DelayUnknown = TimeSpan.FromSeconds(1);

        public static bool IsDelayUnknown(TimeSpan? status) =>
            status.HasValue && status.Value == DelayUnknown;

        public static TimeSpan GetScheduleOffset(TimeSpan? status) =>
            IsDelayUnknown(status) ? TimeSpan.Zero : status ?? TimeSpan.Zero;
    }
}
