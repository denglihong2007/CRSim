using Microsoft.UI.Xaml.Data;

namespace CRSim.Converters
{
    public partial class TicketChecksToStringConverter : IValueConverter
    {
        public string Separator { get; set; } = "";
        public ObservableCollection<WaitingArea> WaitingAreas { get; set; } = [];
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is List<Guid> ticketCheckIds)
            {
                var ticketChecks = WaitingAreas
                        .SelectMany(w => w.TicketChecks
                        .Where(tc => ticketCheckIds.Contains(tc.Id))
                        .Select(tc => $"{w.Name}|{tc.Name}"));
                return string.Join(Separator, ticketChecks);

            }
            return string.Empty;
        }
        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
