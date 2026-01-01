using CRSim.Views.DialogContents;
using Microsoft.Win32;

namespace CRSim.Services
{
    public class DialogService : IDialogService
    {
        public XamlRoot? XamlRoot { get; set; }
        public async Task<string?> GetInputAsync(string title,string placeholder)
        {
            ContentDialog dialog = new()
            {
                XamlRoot = XamlRoot,
                Style = Application.Current.Resources["DefaultContentDialogStyle"] as Style,
                Title = title,
                PrimaryButtonText = "确定",
                CloseButtonText = "取消",
                DefaultButton = ContentDialogButton.Primary,
                Content = new InputDialog(placeholder)
            };
            var result = await dialog.ShowAsync();
            if (result == ContentDialogResult.Primary)
            {
                var inputDialog = dialog.Content as InputDialog;
                string input = inputDialog?.InputText??string.Empty;
                return input;
            }
            return null;
        }
        public async Task<bool> GetConfirmAsync(string title)
        {
            ContentDialog dialog = new()
            {
                XamlRoot = XamlRoot,
                Style = Application.Current.Resources["DefaultContentDialogStyle"] as Style,
                Title = "警告",
                PrimaryButtonText = "确定",
                CloseButtonText = "取消",
                DefaultButton = ContentDialogButton.Primary,
                Content = title
            };
            var result = await dialog.ShowAsync();
            if (result == ContentDialogResult.Primary)
            {
                return true;
            }
            return false;
        }
        public async Task ShowMessageAsync(string title, string message)
        {
            ContentDialog dialog = new()
            {
                XamlRoot = XamlRoot,
                Style = Application.Current.Resources["DefaultContentDialogStyle"] as Style,
                Title = title,
                PrimaryButtonText = "确定",
                DefaultButton = ContentDialogButton.Primary,
                Content = message
            };
            await dialog.ShowAsync();
        }
        public async Task ShowTextAsync(string title, string message)
        {
            ContentDialog dialog = new()
            {
                XamlRoot = XamlRoot,
                Style = Application.Current.Resources["DefaultContentDialogStyle"] as Style,
                Title = title,
                PrimaryButtonText = "确定",
                DefaultButton = ContentDialogButton.Primary,
                Content = new TextDialog(message)
            };
            await dialog.ShowAsync();
        }

        public string? GetFile(string[] extensions)
        {
            var dlg = new OpenFileDialog();
            dlg.Multiselect = false;

            var filters = extensions
                .Select(ext =>
                {
                    var extNoDot = ext.TrimStart('.').ToUpperInvariant();
                    return $"{extNoDot} 文件 (*{ext})|*{ext}";
                });

            dlg.Filter = string.Join("|", filters);

            bool? result = dlg.ShowDialog();
            if (result == true)
            {
                return dlg.FileName;
            }
            return null;
        }

        public async Task<TrainStop?> GetInputTrainNumberStopAsync()
        {
            bool isButtonEnabled = false;
            ContentDialog dialog = new()
            {
                XamlRoot = XamlRoot,
                Style = Application.Current.Resources["DefaultContentDialogStyle"] as Style,
                Title = "编辑车次",
                PrimaryButtonText = "确定",
                CloseButtonText = "取消",
                DefaultButton = ContentDialogButton.Primary,
                IsPrimaryButtonEnabled = false
            };
            var platformDialog = new TrainNumberStopDialog(isValid => dialog.IsPrimaryButtonEnabled = isValid);
            dialog.Content = platformDialog;
            dialog.PrimaryButtonClick += (s, e) =>
            {
                platformDialog.GenerateTrainStop();
            };
            var result = await dialog.ShowAsync();
            if (result == ContentDialogResult.Primary)
            {
                var inputDialog = dialog.Content as TrainNumberStopDialog;
                return inputDialog.GeneratedTrainStop;
            }
            return null;
        }
        public async Task<TrainStop?> EditTrainNumberStopAsync(TrainStop trainStop)
        {
            bool isButtonEnabled = false;
            ContentDialog dialog = new()
            {
                XamlRoot = XamlRoot,
                Style = Application.Current.Resources["DefaultContentDialogStyle"] as Style,
                Title = "编辑车次",
                PrimaryButtonText = "确定",
                CloseButtonText = "取消",
                DefaultButton = ContentDialogButton.Primary,
                IsPrimaryButtonEnabled = false
            };
            var platformDialog = new TrainNumberStopDialog(trainStop, isValid => dialog.IsPrimaryButtonEnabled = isValid);
            dialog.Content = platformDialog;
            dialog.PrimaryButtonClick += (s, e) =>
            {
                platformDialog.GenerateTrainStop();
            };
            var result = await dialog.ShowAsync();
            if (result == ContentDialogResult.Primary)
            {
                var inputDialog = dialog.Content as TrainNumberStopDialog;
                return inputDialog.GeneratedTrainStop;
            }
            return null;
        }

        public async Task<TrainStop?> GetInputTrainStopAsync(List<WaitingArea> waitingAreas, List<string> platforms)
        {
            bool isButtonEnabled = false;
            ContentDialog dialog = new()
            {
                XamlRoot = XamlRoot,
                Style = Application.Current.Resources["DefaultContentDialogStyle"] as Style,
                Title = "编辑车次",
                PrimaryButtonText = "确定",
                CloseButtonText = "取消",
                DefaultButton = ContentDialogButton.Primary,
                IsPrimaryButtonEnabled = false
            };
            var platformDialog = new TrainStopDialog(waitingAreas, platforms,isValid => dialog.IsPrimaryButtonEnabled = isValid);
            dialog.Content = platformDialog;
            dialog.PrimaryButtonClick += (s, e) =>
            {
                platformDialog.GenerateTrainStop();
            };
            var result = await dialog.ShowAsync();
            if (result == ContentDialogResult.Primary)
            {
                var inputDialog = dialog.Content as TrainStopDialog;
                return inputDialog.GeneratedTrainStop;
            }
            return null;
        }

        public async Task<TrainStop?> EditInputTrainStopAsync(List<WaitingArea> waitingAreas, List<string> platforms, TrainStop trainStop)
        {
            bool isButtonEnabled = false;
            ContentDialog dialog = new()
            {
                XamlRoot = XamlRoot,
                Style = Application.Current.Resources["DefaultContentDialogStyle"] as Style,
                Title = "编辑车次",
                PrimaryButtonText = "确定",
                CloseButtonText = "取消",
                DefaultButton = ContentDialogButton.Primary,
                IsPrimaryButtonEnabled = false
            };
            var platformDialog = new TrainStopDialog(waitingAreas, platforms, trainStop, isValid => dialog.IsPrimaryButtonEnabled = isValid);
            dialog.Content = platformDialog;
            dialog.PrimaryButtonClick += (s, e) =>
            {
                platformDialog.GenerateTrainStop();
            };
            var result = await dialog.ShowAsync();
            if (result == ContentDialogResult.Primary)
            {
                var inputDialog = dialog.Content as TrainStopDialog;
                return inputDialog.GeneratedTrainStop;
            }
            return null;
        }

        public string? SaveFile(string extension, string suggestedFileName)
        {
            var dlg = new SaveFileDialog
            {
                FileName = suggestedFileName
            };
            var extNoDot = extension.TrimStart('.').ToUpperInvariant();
            var filter = $"{extNoDot} Files (*{extension})|*{extension}";
            dlg.Filter = filter;
            bool? result = dlg.ShowDialog();
            if (result == true)
            {
                return dlg.FileName;
            }
            return null;
        }

        public async Task<List<Platform>?> GetInputPlatformAsync()
        {
            bool isButtonEnabled = false;
            ContentDialog dialog = new()
            {
                XamlRoot = XamlRoot,
                Style = Application.Current.Resources["DefaultContentDialogStyle"] as Style,
                Title = "新增站台",
                PrimaryButtonText = "确定",
                CloseButtonText = "取消",
                DefaultButton = ContentDialogButton.Primary,
                IsPrimaryButtonEnabled = false
            };
            var platformDialog = new PlatformDialog(isValid => dialog.IsPrimaryButtonEnabled = isValid);
            dialog.Content = platformDialog;
            dialog.PrimaryButtonClick += (s, e) =>
            {
                platformDialog.GeneratePlatform();
            };
            var result = await dialog.ShowAsync();
            if (result == ContentDialogResult.Primary)
            {
                var inputDialog = dialog.Content as PlatformDialog;
                return inputDialog.GeneratedPlatforms;
            }
            return null;
        }

        public async Task<Station?> CreateStationAsync()
        {
            ContentDialog dialog = new()
            {
                XamlRoot = XamlRoot,
                Style = Application.Current.Resources["DefaultContentDialogStyle"] as Style,
                Title = "新增车站",
                PrimaryButtonText = "确定",
                CloseButtonText = "取消",
                DefaultButton = ContentDialogButton.Primary,
                IsPrimaryButtonEnabled = false
            };
            var createStationDialog = new CreateStationDialog(isValid => dialog.IsPrimaryButtonEnabled = isValid);
            dialog.Content = createStationDialog;
            var result = await dialog.ShowAsync();
            if (result == ContentDialogResult.Primary)
            {
                var inputDialog = dialog.Content as CreateStationDialog;
                return inputDialog.GeneratedStation;
            }
            return null;
        }

        public async Task<List<TrainColor>?> EditTrainColorsAsync(List<TrainColor> trainColors)
        {
            ContentDialog dialog = new()
            {
                XamlRoot = XamlRoot,
                Style = Application.Current.Resources["DefaultContentDialogStyle"] as Style,
                Title = "编辑颜色配置",
                PrimaryButtonText = "确定",
                CloseButtonText = "取消",
                DefaultButton = ContentDialogButton.Primary,
                IsPrimaryButtonEnabled = false
            };
            var trainColorsDialog = new TrainColorsDialog(trainColors, isValid => dialog.IsPrimaryButtonEnabled = isValid);
            dialog.Content = trainColorsDialog;
            var result = await dialog.ShowAsync();
            if (result == ContentDialogResult.Primary)
            {
                var inputDialog = dialog.Content as TrainColorsDialog;
                return [.. inputDialog.TrainColors];
            }
            return null;
        }
    }
}
