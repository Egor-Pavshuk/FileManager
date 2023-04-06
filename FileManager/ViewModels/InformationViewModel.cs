using FileManager.Helpers;
using System;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.ApplicationModel.Resources;
using Windows.Storage;
using Windows.System;
using Windows.System.Power;
using Windows.UI.Core;

namespace FileManager.ViewModels
{
    public class InformationViewModel : BindableBase
    {
        private string batteryImage;
        private double batteryLevel;
        private string freeSpaceGb;
        private string ramMemoryUsed;
        private string batteryLevelPercentage;
        private ResourceLoader batteryResourceLoader;
        private ResourceLoader stringsResourceLoader;
        public string BatteryImage
        {
            get => batteryImage;
            set
            {
                if (batteryImage != value)
                {
                    batteryImage = value;
                    OnPropertyChanged();
                }
            }
        }
        public double BatteryLevel
        {
            get => batteryLevel;
            set
            {
                if (batteryLevel != value)
                {
                    batteryLevel = value;
                    OnPropertyChanged();
                }
            }
        }
        public string BatteryLevelPercentage
        {
            get => batteryLevelPercentage;
            set
            {
                if (batteryLevelPercentage != value)
                {
                    batteryLevelPercentage = value;
                    OnPropertyChanged();
                }
            }
        }
        public string FreeSpaceGb
        {
            get => freeSpaceGb;
            set
            {
                if (freeSpaceGb != value)
                {
                    freeSpaceGb = value;
                    OnPropertyChanged();
                }
            }
        }
        public string RamMemoryUsed
        {
            get => ramMemoryUsed;
            set
            {
                if (ramMemoryUsed != value)
                {
                    ramMemoryUsed = value;
                    OnPropertyChanged();
                }
            }
        }
        public InformationViewModel()
        {
            batteryResourceLoader = ResourceLoader.GetForCurrentView(Constants.Batteries);
            stringsResourceLoader = ResourceLoader.GetForCurrentView(Constants.Resources);
            BatteryTrigger();
            MemoryTrigger();
            UpdateMemoryStatus().ConfigureAwait(true);
            UpdateBatteryStatus().ConfigureAwait(true);
            GetFreeSpace();
        }

        public void BatteryTrigger()
        {
            Windows.Devices.Power.Battery.AggregateBattery.ReportUpdated
            += async (sender, args) => await UpdateBatteryStatus().ConfigureAwait(true);
        }

        public void MemoryTrigger()
        {
            MemoryManager.AppMemoryUsageDecreased
            += async (sender, args) => await UpdateMemoryStatus().ConfigureAwait(true);
            MemoryManager.AppMemoryUsageIncreased
            += async (sender, args) => await UpdateMemoryStatus().ConfigureAwait(true);
        }

        private async Task UpdateBatteryStatus()
        {
            await CoreApplication.MainView.CoreWindow.Dispatcher
                .RunAsync(CoreDispatcherPriority.Normal,
            () =>
            {
                var batteryReport = Windows.Devices.Power.Battery.AggregateBattery.GetReport();

                double percentage;
                try
                {
                    percentage = (batteryReport.RemainingCapacityInMilliwattHours.Value /
                (double)batteryReport.FullChargeCapacityInMilliwattHours.Value);
                }
                catch (InvalidOperationException)
                {
                    percentage = 1;
                }

                BatteryLevel = percentage * 100;
                BatteryLevelPercentage = $"{(int)BatteryLevel} %";

                switch (batteryReport.Status)
                {
                    case BatteryStatus.Idle:
                        BatteryImage = batteryResourceLoader.GetString(Constants.FullBattery);
                        break;
                    case BatteryStatus.Charging:
                        BatteryImage = batteryResourceLoader.GetString(Constants.BatteryCharge);
                        break;
                    case BatteryStatus.Discharging:
                        if (batteryLevel <= 100 && BatteryLevel > 76)
                        {
                            BatteryImage = batteryResourceLoader.GetString(Constants.FullBattery);
                        }
                        else if (BatteryLevel <= 76 && BatteryLevel > 51)
                        {
                            BatteryImage = batteryResourceLoader.GetString(Constants.Battery);
                        }
                        else if (BatteryLevel <= 51 && BatteryLevel > 31)
                        {
                            BatteryImage = batteryResourceLoader.GetString(Constants.Halfbattery);
                        }
                        else if (BatteryLevel <= 31 && BatteryLevel > 21)
                        {
                            BatteryImage = batteryResourceLoader.GetString(Constants.LowBattery);
                        }
                        else
                        {
                            BatteryImage = batteryResourceLoader.GetString(Constants.EmptyBattery);
                        }
                        break;
                    default:
                        BatteryImage = batteryResourceLoader.GetString(Constants.BatteryAttention);
                        break;
                }
            });
        }

        private async Task UpdateMemoryStatus()
        {
            await CoreApplication.MainView.CoreWindow.Dispatcher
                .RunAsync(CoreDispatcherPriority.Normal,
            () =>
            {
                var usageInB = MemoryManager.AppMemoryUsage;
                var usageInKB = usageInB / 1024.0;
                var usageInMB = usageInKB / 1024.0;

                RamMemoryUsed = stringsResourceLoader.GetString(Constants.MemoryUsage) + $": {Math.Round(usageInMB, 2)} Mb";
            });
        }

        private async void GetFreeSpace()
        {
            var retrieveProperties = await ApplicationData.Current.LocalFolder.Properties.RetrievePropertiesAsync(new string[] 
            { 
                Constants.FreeSpaceKey 
            });
            var freeSpaceRemaining = retrieveProperties[Constants.FreeSpaceKey];

            var sizeInKB = (ulong)freeSpaceRemaining / 1024.0;
            var sizeInMB = sizeInKB / 1024.0;
            var sizeInGb = sizeInMB / 1024.0;

            FreeSpaceGb = stringsResourceLoader.GetString(Constants.FreeSpace) + $": {Math.Round(sizeInGb, 2)} Gb";
        }
    }
}
