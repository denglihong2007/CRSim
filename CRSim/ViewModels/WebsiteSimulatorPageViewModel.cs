using CRSim.WebsiteSimulator;

namespace CRSim.ViewModels;

public partial class WebsiteSimulatorPageViewModel(IDialogService _dialogService, Simulator _simulator) : ObservableObject
{
    public string PageTitle = "12306模拟";

    [RelayCommand]
    public async Task StartSimulation()
    {
        try
        {
            await _simulator.Start();
        }
        catch (Exception e)
        {
            await _dialogService.ShowMessageAsync("认证失败", e.Message);
        }
    }

    [RelayCommand]
    public void StopSimulation()
    {
        _simulator.Stop();
    }
}