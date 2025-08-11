namespace CRSim.Selectors
{
    public partial class TreeViewTemplateSelector : DataTemplateSelector
    {
        public DataTemplate WaitingAreaTemplate { get; set; }
        public DataTemplate TicketCheckTemplate { get; set; }

        protected override DataTemplate SelectTemplateCore(object item)
        {
            return item switch
            {
                WaitingArea => WaitingAreaTemplate,
                TicketCheck => TicketCheckTemplate,
                _ => base.SelectTemplateCore(item)
            };
        }
    }
}
