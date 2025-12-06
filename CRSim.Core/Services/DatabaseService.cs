using CRSim.Core.Abstractions;
using CRSim.Core.Models;
using CRSim.Core.Utils;
using System.Text.Json;

namespace CRSim.Core.Services
{
    public class DatabaseService : IDatabaseService
    {
        private List<Station> _stations;
        private List<TrainNumber> _trainNumbers;

        public List<Station> GetAllStations()
        {
            return _stations;
        }

        public void ImportData(string jsonFilePath)
        {
            try
            {
                var json =  File.ReadAllText(jsonFilePath).Replace("StationStop", "TrainStop");
                var data = JsonSerializer.Deserialize<Json>(json,JsonContext.Default.Json);
                _stations = data.Stations;
                _trainNumbers = data.TrainNumbers;
            }
            catch
            {
                _stations = [];
                _trainNumbers = [];
            }
        }
        public Station GetStationByName(string name)
        {
            return _stations.FirstOrDefault(x => x.Name == name);
        }
        public TrainNumber GetTrainNumberByNumber(string number)
        {
            return _trainNumbers.FirstOrDefault(x => x.Number == number);
        }

        public void DeleteStation(Station station)
        {
            _stations.Remove(station);
        }

        public void DeleteTrainNumber(TrainNumber trainNumber)
        {
            _trainNumbers.Remove(trainNumber);
        }

        public void UpdateStation(string stationName,Station station)
        {
            var targetStation = _stations.FirstOrDefault(x => x.Name == stationName);
            if (targetStation != null)
            {
                // 直接更新 targetStation 中的内容
                targetStation.Name = station.Name;
                targetStation.WaitingAreas = station.WaitingAreas;
                targetStation.TrainStops = station.TrainStops;
                targetStation.Platforms = station.Platforms;
            }
        }

        public void UpdateTrainNumber(TrainNumber trainNumber, List<TrainStop> timeTable, List<Section>? sections)
        {
            var targetTrainNumber = _trainNumbers.FirstOrDefault(x => x.Number == trainNumber.Number);
            targetTrainNumber.TimeTable = timeTable;
            if (sections!= null)
            {
                targetTrainNumber.Sections = sections;
            }
        }

        public async Task SaveData()
        {
            string json = JsonSerializer.Serialize(new Json()
            { 
                Stations = _stations,
                TrainNumbers = _trainNumbers
            },JsonContext.Default.Json);
            await File.WriteAllTextAsync(AppPaths.ConfigFilePath, json);
        }

        public async Task ExportData(string p)
        {
            string json = JsonSerializer.Serialize(new Json()
            {
                Stations = _stations,
                TrainNumbers = _trainNumbers
            }, JsonContext.Default.Json);
            await File.WriteAllTextAsync(p, json);
        }

        public List<TrainNumber> GetAllTrainNumbers()
        {
            return _trainNumbers;
        }

        public void AddTrainNumber(TrainNumber trainNumber)
        {
            _trainNumbers.Add(trainNumber);
        }

        public void AddStation(Station station)
        {
            _stations.Add(station);
        }

        public void ClearData()
        {
            _stations.Clear();
            _trainNumbers.Clear();
        }
    }
}
