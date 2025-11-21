namespace CRSim.Controls;

public sealed partial class HomePageHeader : UserControl
{
    public List<string> CarouselImages { get; set; }
    private DispatcherTimer _timer;

    public HomePageHeader()
    {
        InitializeComponent();
        CarouselImages = new List<string>
        {
            "ms-appx:///Assets/HomeHeaderTiles/GalleryHeaderImage-Republic.jpg"
        };
        this.DataContext = this;

        _timer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(5)
        };
        _timer.Tick += (s, e) =>
        {
            var total = HeroImageFlipView.Items.Count;
            var next = (HeroImageFlipView.SelectedIndex + 1) % total;
            HeroImageFlipView.SelectedIndex = next;
        };
        _timer.Start();
    }
}