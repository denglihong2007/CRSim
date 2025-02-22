namespace CRSim.ViewModels
{
    public partial class DataManagementPageViewModel(INavigationService navigationService) : ObservableObject
    {
        [ObservableProperty]
        private string _pageTitle = "���ݹ���";

        [ObservableProperty]
        private string _pageDescription = "��վ�����Ρ���·����Ϣ����";

        [ObservableProperty]
        private ICollection<ControlInfoDataItem> _navigationCards = ControlsInfoDataSource.Instance.GetControlsInfo("Data Management");

        private readonly INavigationService _navigationService = navigationService;

        [RelayCommand]
        public void Navigate(object pageType){
            if (pageType is Type page)
            {
                _navigationService.NavigateTo(page);
            }
        }

        
    }
}
