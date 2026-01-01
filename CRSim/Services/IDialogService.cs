namespace CRSim.Services
{
    public interface IDialogService
    {
        string? GetFile(string[] filter);
        string? SaveFile(string filter,string fileName);
        Task<Station?> CreateStationAsync();
        Task<string?> GetInputAsync(string title,string placeholder);
        Task<bool> GetConfirmAsync(string title);
        Task ShowMessageAsync(string title, string message);
        Task ShowTextAsync(string title, string message);
        Task<TrainStop?> GetInputTrainNumberStopAsync();
        Task<TrainStop?> EditTrainNumberStopAsync(TrainStop trainStop);
        Task<TrainStop?> GetInputTrainStopAsync(List<WaitingArea> waitingAreas, List<string> platforms);
        Task<TrainStop?> EditInputTrainStopAsync(List<WaitingArea> waitingAreas, List<string> platforms, TrainStop trainStop);
        Task<List<Platform>?> GetInputPlatformAsync();
        Task<List<TrainColor>?> EditTrainColorsAsync(List<TrainColor> trainColors);
        XamlRoot? XamlRoot { get; set; }
    }
}
