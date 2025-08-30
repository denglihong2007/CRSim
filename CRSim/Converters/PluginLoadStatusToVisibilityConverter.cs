using CRSim.Core.Enums;
using CRSim.Core.Models.Plugin;
using Microsoft.UI.Xaml.Data;

namespace CRSim.Converters
{
    public partial class PluginLoadStatusToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if(value is PluginInfo plugin && parameter is string target)
            {
                PluginLoadStatus status = plugin.LoadStatus;
                if ((plugin.IsUninstalling && target== "Loaded") || (plugin.RestartRequired && target == "NotLoaded"))
                {
                    return Visibility.Collapsed;
                }
                if ((status == PluginLoadStatus.Loaded && target == "Loaded") ||
                    (status == PluginLoadStatus.Error && target == "Error") ||
                    (status == PluginLoadStatus.NotLoaded && target == "NotLoaded") ||
                    (status == PluginLoadStatus.Disabled && target == "Disabled"))
                {
                    return Visibility.Visible;
                }
            }
            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
