using CRSim.Core.Services;
using CRSim.Core.Models;
using CRSim.ScreenSimulator.Models;
using System.Windows;
namespace CRSim.ScreenSimulator.ViewModels.Zibo
{
    public class ExitScreenViewModel : ScreenViewModel
    {
        public ExitScreenViewModel(ITimeService timeService, ISettingsService settingsService)
            : base(timeService, settingsService)
        {
            Text = $"温馨提示：请接站的旅客在出站口外耐心等候";
            ItemsPerPage = 6;
            ScreenCount = 2;
            StationType = StationType.Arrival;
        }
        public override void RefreshDisplay(object? sender, EventArgs? e)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                int pageCount = (int)Math.Ceiling((double)TrainInfo.Count / (ItemsPerPage * ScreenCount.Value));
                ScreenA.Clear();
                ScreenB.Clear();
                int startIndex = CurrentPageIndex * ItemsPerPage * ScreenCount.Value;
                var ItemsToShow = TrainInfo.Skip(startIndex).Take(ItemsPerPage * ScreenCount.Value).ToList();
                var leftItems = ItemsToShow.Where((item, index) => index % 2 == 0).ToList();
                var rightItems = ItemsToShow.Where((item, index) => index % 2 == 1).ToList();
                while (leftItems.Count < ItemsPerPage)
                {
                    leftItems.Add(new TrainInfo());
                }
                while (rightItems.Count < ItemsPerPage)
                {
                    rightItems.Add(new TrainInfo());
                }
                foreach (var item in leftItems)
                {
                    ScreenA.Add(item);
                }
                foreach (var item in rightItems)
                {
                    ScreenB.Add(item);
                }
                CurrentPageIndex = CurrentPageIndex + 1 >=  Math.Min(_settings.MaxPages, pageCount) ? 0 : CurrentPageIndex + 1;
            });
        }
    }
}
