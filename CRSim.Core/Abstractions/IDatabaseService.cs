using CRSim.Core.Models;
using Section = CRSim.Core.Models.Section;

namespace CRSim.Core.Abstractions
{
    public interface IDatabaseService
    {
        List<Station> GetAllStations();
        List<TrainNumber> GetAllTrainNumbers();
        Station GetStationByName(string name);
        TrainNumber GetTrainNumberByNumber(string number);
        void AddStation(Station station);
        void AddTrainNumber(TrainNumber trainNumber);
        void DeleteStation(Station station);
        void DeleteTrainNumber(TrainNumber trainNumber);
        void UpdateStation(string stationName,Station station);
        void UpdateTrainNumber(TrainNumber trainNumber, List<TrainStop> timeTable, List<Section>? sections);
        Task SaveData();
        void ImportData(string jsonFilePath);
        Task ExportData(string path);
        void ClearData();
    }
}
