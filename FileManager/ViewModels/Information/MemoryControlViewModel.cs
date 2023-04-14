using System;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.System;
using Windows.UI.Core;
using FileManager.Helpers;

namespace FileManager.ViewModels.Information
{
    public class MemoryControlViewModel : InformationControlViewModel
    {
        public MemoryControlViewModel()
        {
            Background = "LightBlue";
            Image = themeResourceLoader.GetString(Constants.RamIcon);
            IsProgressBarVisible = false;
            UpdateMemoryStatus().ConfigureAwait(true);
        }
        public override async Task UpdateMemoryStatus()
        {
            await CoreApplication.MainView.CoreWindow.Dispatcher
                .RunAsync(CoreDispatcherPriority.Normal,
            () =>
            {
                var usageInB = MemoryManager.AppMemoryUsage;
                var usageInKB = usageInB / 1024.0;
                var usageInMB = usageInKB / 1024.0;

                Text = stringsResourceLoader.GetString(Constants.MemoryUsage) + $": {Math.Round(usageInMB, 2)} Mb";
            });
        }
    }
}
