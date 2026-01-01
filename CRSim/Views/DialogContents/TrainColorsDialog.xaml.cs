using System.Drawing;

namespace CRSim.Views.DialogContents;
public sealed partial class TrainColorsDialog : Page
{
    public ObservableCollection<TrainColor> TrainColors = [];

    private readonly Action<bool> _onValidityChanged;

    public TrainColorsDialog(List<TrainColor> trainColors, Action<bool> onValidityChanged)
    {
        TrainColors = new ObservableCollection<TrainColor>(trainColors);
        InitializeComponent();
        _onValidityChanged = onValidityChanged;
        Validate(this, EventArgs.Empty);
    }

    private void Add(object sender, RoutedEventArgs e)
    {
        TrainColors.Add(new TrainColor() { Color = Color.Red, Prefix = string.Empty });
        Validate(this, EventArgs.Empty);
    }

    private void Delete(object sender, RoutedEventArgs e)
    {
        if (TrainColorsGrid.SelectedValue is TrainColor trainColor && trainColor.Prefix != "默认")
        {
            TrainColors.Remove(trainColor);
        }
        Validate(this, EventArgs.Empty);
    }

    private void Reset(object sender, RoutedEventArgs e)
    {
        TrainColors.Clear();
        foreach (var trainColor in new Settings().TrainColors)
        {
            TrainColors.Add(trainColor);
        }
        Validate(this, EventArgs.Empty);
    }

    private void Validate(object sender, object e)
    {
        if(TrainColorsGrid is null) { return; } //检查是否初始化完成
        if (TrainColors.Any(x => x.Prefix == string.Empty) || TrainColors.GroupBy(x => x.Prefix).Any(g => g.Count() > 1) || !TrainColors.Any(x=>x.Prefix=="默认"))
        {
            _onValidityChanged(false);
        }
        else
        {
            _onValidityChanged(true);
        }
    }
}
