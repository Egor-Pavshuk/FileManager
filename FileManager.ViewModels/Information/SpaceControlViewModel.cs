using System;
using System.Threading.Tasks;
using Windows.Storage;
using FileManager.Helpers;

namespace FileManager.ViewModels.Information
{
    public class SpaceControlViewModel : InformationControlViewModel
    {
        public SpaceControlViewModel()
        {
            IsProgressBarVisible = false;
            Background = "#FFF851";
            Image = themeResourceLoader.GetString(Constants.DiskStorageIcon);
            GetFreeSpaceAsync().ConfigureAwait(true);
        }
        public override async Task GetFreeSpaceAsync()
        {
            var retrieveProperties = await ApplicationData.Current.LocalFolder.Properties.RetrievePropertiesAsync(new string[]
            {
                Constants.FreeSpaceKey
            });
            var freeSpaceRemaining = retrieveProperties[Constants.FreeSpaceKey];

            var sizeInKB = (ulong)freeSpaceRemaining / 1024.0;
            var sizeInMB = sizeInKB / 1024.0;
            var sizeInGb = sizeInMB / 1024.0;

            Text = stringsResourceLoader.GetString(Constants.FreeSpace) + $": {Math.Round(sizeInGb, 2)} Gb";
        }
    }
}
