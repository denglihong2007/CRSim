using System.Globalization;
using System.Windows.Data;

namespace CRSim.ScreenSimulator.Converters
{
    public class NumberToImageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return null;
            string trainNumber = value.ToString();
            string imageName = "pack://application:,,,/CRSim.ScreenSimulator;component/Assets/logo-GTJT.png";
            if (!string.IsNullOrEmpty(trainNumber) && (trainNumber.StartsWith('G') || trainNumber.StartsWith('C') || trainNumber.StartsWith('D')))
            {
                int probability = trainNumber.GetHashCode() % 2;
                if (probability == 1)
                {
                    imageName = "pack://application:,,,/CRSim.ScreenSimulator;component/Assets/logo-CR.png";
                }
                else
                {
                    imageName = "pack://application:,,,/CRSim.ScreenSimulator;component/Assets/logo-CRH.png";
                }
            }
            var image = new System.Windows.Media.Imaging.BitmapImage();
            image.BeginInit();
            image.UriSource = new Uri(imageName, UriKind.Absolute);
            image.CacheOption = System.Windows.Media.Imaging.BitmapCacheOption.OnLoad;
            image.EndInit();
            return image;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
