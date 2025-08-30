namespace CRSim.Views;
public sealed partial class PlatformDialog : Page
{
    private readonly Action<bool> _onValidityChanged;
    public List<Platform> GeneratedPlatforms = [];
    private void Validate(object sender, object e)
    {
        bool isValid = !string.IsNullOrWhiteSpace(PlatformTextBox.Text);
        _onValidityChanged?.Invoke(isValid);
    }
    public PlatformDialog(Action<bool> onValidityChanged)
    {
        _onValidityChanged = onValidityChanged;
        InitializeComponent();
    }
    public void GeneratePlatform()
    {
        string[] platforms = PlatformTextBox.Text.Split('-');
        int Length = (int)LengthNumberBox.Value;
        if (platforms.Length == 2 && int.TryParse(platforms[0], out int startindex) && int.TryParse(platforms[1], out int endindex))
        {
            var startIndex = Math.Min(startindex, endindex);
            var endIndex = Math.Max(startindex, endindex);
            for (int i = startIndex; i < endIndex + 1; i++)
            {
                GeneratedPlatforms.Add(new Platform { Name = i.ToString(), Length = Length });
            }
        }
        else
        {
            GeneratedPlatforms.Add(new Platform { Name = platforms[0], Length = Length });
        }
    }
}
