namespace CRSim.ScreenSimulator.Models
{
    public class TrainInfo
    {
        public string TrainNumber { get; set; }
        public string Origin { get; set; }
        public string Terminal { get; set; }
        private DateTime? arrivalTime { get; set; }
        private DateTime? departureTime { get; set; }
        public DateTime? ArrivalTime
        {
            get 
            {
                return State is null ? arrivalTime : arrivalTime + State;
            } 
            set 
            {
                arrivalTime = value;
            }
        }
        public DateTime? DepartureTime
        {
            get
            {
                return State is null || State < TimeSpan.Zero ? departureTime : departureTime + State;
            }
            set
            {
                departureTime = value;
            }
        }
        public List<string> TicketChecks { get; set; }
        public string WaitingArea { get; set; }
        public string Platform { get; set; }
        public TimeSpan? State { get; set; }
        public string? Landmark { get; set; }
        public int Length { get; set; }
    }
}
