using CRSim.Core.Abstractions;
using CRSim.Core.Models;
using CRSim.Core.Utils;
using System.Text.Json;
using System.Text.Json.Serialization;

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
        public void AddStationByName(string name)
        {
            _stations.Add(new Station()
            {
                Name = name,
            });
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

        public void AddTrainNumber(string number)
        {
            _trainNumbers.Add(new TrainNumber()
            {
                Number = number
            });
        }

        public async Task ImportStationFrom7D(string path)
        {
            Station station = new() { Platforms = [], WaitingAreas = [], TrainStops = [] };
            var stationName = (await File.ReadAllTextAsync(path.Replace("车站广播系统.exe", "info.ini"))).Split("|")[0];
            if (_stations.Any(x => x.Name == stationName))
            {
                _stations.Remove(GetStationByName(stationName));
            }
            var lines = await File.ReadAllLinesAsync(path.Replace("车站广播系统.exe", "data.csv"));
            station.Name = stationName;
            foreach (var line in lines)
            {
                var data = line.Split(',');
                var waitingAreaName = string.IsNullOrWhiteSpace(data.Last()) ? "未知" : data.Last();
                var platform = string.IsNullOrWhiteSpace(data[5]) ? "未知" : data[5];
                var ticketCheck = string.IsNullOrWhiteSpace(data[4]) ? string.Empty : data[4];

                var existingPlatform = station.Platforms.FirstOrDefault(x => x.Name == platform);
                if (existingPlatform == null)
                {
                    station.Platforms.Add(new Platform { Name = platform, Length = 20 });
                }


                var waitingArea = station.WaitingAreas.FirstOrDefault(x => x.Name == waitingAreaName);
                if (waitingArea == null)
                {
                    waitingArea = new WaitingArea() { Name = waitingAreaName };
                    station.WaitingAreas.Add(waitingArea);
                }

                foreach (var ticketCheckName in ticketCheck.Split('|'))
                {
                    if (!string.IsNullOrWhiteSpace(ticketCheckName) && !waitingArea.TicketChecks.Any(x => x.Name == ticketCheckName))
                    {
                        waitingArea.TicketChecks.Add(new TicketCheck { Name = ticketCheckName });
                    }
                }

                if (!string.IsNullOrEmpty(data[0]) && !station.TrainStops.Any(x => x.Number == data[0]))
                {
                    TimeSpan? arrivalTime = null;
                    TimeSpan? departureTime = null;

                    if (data[2] == stationName)
                    {
                        departureTime = TimeSpan.Parse(lines.FirstOrDefault(x => x.Split(",")[0] == data[0] && x.Split(",")[1].Contains("送车"))?.Split(",")[7] ?? string.Empty);
                    }
                    else if (data[3] == stationName)
                    {
                        arrivalTime = TimeSpan.Parse(data[7]);
                    }
                    else
                    {
                        arrivalTime = TimeSpan.Parse(data[7]);
                        departureTime = TimeSpan.Parse(lines.FirstOrDefault(x => x.Split(",")[0] == data[0] && x.Split(",")[1].Contains("送车"))?.Split(",")[7] ?? string.Empty);
                    }

                    station.TrainStops.Add(new TrainStop
                    {
                        Number = data[0],
                        Terminal = data[3],
                        Origin = data[2],
                        TicketCheckIds = [.. waitingArea.TicketChecks.Select(x=>x.Id)],
                        ArrivalTime = arrivalTime,
                        DepartureTime = departureTime,
                        Platform = platform,
                        Length = data[0].StartsWith('G') || data[0].StartsWith('D') || data[0].StartsWith('C') ? Math.Abs(data[0].GetHashCode()) % 3 == 0 ? 8 : 16 : 18,
                        Landmark = data[8] + "色" ?? null,
                    });
                }
            }
            AddStation(station);
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
