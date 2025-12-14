using CRSim.Core.Abstractions;
using CRSim.Core.Models;
using CRSim.Core.Utils;
using System;
using System.IO;
using System.Text.Json;

namespace CRSim.Core.Services
{
    public class SettingsService : ISettingsService
    {
        private Settings _settings;
        private readonly string _settingsFilePath;

        public SettingsService()
        {
            _settingsFilePath = Path.Combine(AppPaths.AppDataPath, "settings.json");
        }

        public void SaveSettings()
        {
            // Ensure the directory exists
            Directory.CreateDirectory(AppPaths.AppDataPath);

            _settings.ApiName = _settings.Api.Name;

            var options = new JsonSerializerOptions { WriteIndented = true };
            string jsonString = JsonSerializer.Serialize(_settings, options);
            File.WriteAllText(_settingsFilePath, jsonString);
        }

        public void LoadSettings()
        {
            try
            {
                if (File.Exists(_settingsFilePath))
                {
                    string jsonString = File.ReadAllText(_settingsFilePath);
                    _settings = JsonSerializer.Deserialize<Settings>(jsonString);
                    _settings.Api = new ApiFactory().CreateApi(_settings.ApiName);
                }
                else
                {
                    _settings = new Settings();
                    SaveSettings();
                }
            }
            catch (JsonException)
            {
                // Handle the case where the json is corrupted, load default settings
                _settings = new Settings();
                SaveSettings();
            }
        }

        public Settings GetSettings()
        {
            return _settings;
        }
    }
}
