namespace CRSim.Views;
public sealed partial class TextDialog : Page
{
    private string _text { get; set; }
    public TextDialog(string text)
    {
        _text = text;
        InitializeComponent();
    }
}
