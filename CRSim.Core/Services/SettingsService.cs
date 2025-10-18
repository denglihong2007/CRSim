using CRSim.Core.Abstractions;
using CRSim.Core.Models;
using Microsoft.Win32;

namespace CRSim.Core.Services
{
    public class SettingsService : ISettingsService
    {
        private Settings _settings;
        private RegistryKey _key = Registry.CurrentUser.OpenSubKey(@"Software\CRSim\Settings",true);
        public void SaveSettings()
        {
            _key.SetValue("SwitchPageSeconds", _settings.SwitchPageSeconds);
            _key.SetValue("Api", _settings.Api.Name);
            _key.SetValue("MaxPages", _settings.MaxPages);
            _key.SetValue("StopCheckInAdvanceDuration", (int)_settings.StopCheckInAdvanceDuration.TotalMinutes);
            _key.SetValue("StopDisplayUntilDepartureDuration", (int)_settings.StopDisplayUntilDepartureDuration.TotalMinutes);
            _key.SetValue("StopDisplayFromArrivalDuration", (int)_settings.StopDisplayFromArrivalDuration.TotalMinutes);
            _key.SetValue("PassingCheckInAdvanceDuration", (int)_settings.PassingCheckInAdvanceDuration.TotalMinutes);
            _key.SetValue("DepartureCheckInAdvanceDuration", (int)_settings.DepartureCheckInAdvanceDuration.TotalMinutes);
            _key.SetValue("UserKey", _settings.UserKey);
            _key.SetValue("LoadTodayOnly", _settings.LoadTodayOnly);
            _key.SetValue("ReopenUnclosedScreensOnLoad", _settings.ReopenUnclosedScreensOnLoad);
        }
        public void LoadSettings()
        {
            if (_key==null)
            {
                Registry.CurrentUser.CreateSubKey(@"Software\CRSim\Settings");
                _key = Registry.CurrentUser.OpenSubKey(@"Software\CRSim\Settings",true);
                _settings = new Settings();
                SaveSettings();
            }
            else
            {
                _settings = new Settings();
                if (_key.GetValue("SwitchPageSeconds") != null) _settings.SwitchPageSeconds = (int)_key.GetValue("SwitchPageSeconds");
                _settings.Api = new ApiFactory().CreateApi((string)_key.GetValue("Api")); 
                if (_key.GetValue("MaxPages") != null) _settings.MaxPages = (int)_key.GetValue("MaxPages");
                if (_key.GetValue("StopCheckInAdvanceDuration") != null) _settings.StopCheckInAdvanceDuration = TimeSpan.FromMinutes((int)_key.GetValue("StopCheckInAdvanceDuration"));
                if (_key.GetValue("StopDisplayUntilDepartureDuration") != null) _settings.StopDisplayUntilDepartureDuration = TimeSpan.FromMinutes((int)_key.GetValue("StopDisplayUntilDepartureDuration"));
                if (_key.GetValue("StopDisplayFromArrivalDuration") != null) _settings.StopDisplayFromArrivalDuration = TimeSpan.FromMinutes((int)_key.GetValue("StopDisplayFromArrivalDuration"));
                if (_key.GetValue("PassingCheckInAdvanceDuration") != null) _settings.PassingCheckInAdvanceDuration = TimeSpan.FromMinutes((int)_key.GetValue("PassingCheckInAdvanceDuration"));
                if (_key.GetValue("DepartureCheckInAdvanceDuration") != null) _settings.DepartureCheckInAdvanceDuration = TimeSpan.FromMinutes((int)_key.GetValue("DepartureCheckInAdvanceDuration"));
                if (_key.GetValue("UserKey") != null) _settings.UserKey = (string)_key.GetValue("UserKey");
                if (_key.GetValue("LoadTodayOnly") != null) _settings.LoadTodayOnly = bool.Parse((string)_key.GetValue("LoadTodayOnly"));
                if (_key.GetValue("ReopenUnclosedScreensOnLoad") != null) _settings.ReopenUnclosedScreensOnLoad = bool.Parse((string)_key.GetValue("ReopenUnclosedScreensOnLoad"));
            }
        }

        public Settings GetSettings()
        {
            return _settings;
        }
    }
}
