namespace CRSim.Views;
public sealed partial class InputDialog : Page
{
    public string InputText { get; set; }
    public string Placeholder { get; set; }
    public InputDialog(string placeholder)
    {
        Placeholder = placeholder;
        InitializeComponent();
    }
}
