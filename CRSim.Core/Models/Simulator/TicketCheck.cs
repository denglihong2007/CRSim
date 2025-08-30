namespace CRSim.Core.Models
{
    public class TicketCheck
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; }
        public override string ToString() => Name;
    }
}