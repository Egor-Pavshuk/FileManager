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
        private const string batteries = "Batteries";
        private const string resources = "Resources";
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
            batteryResourceLoader = ResourceLoader.GetForCurrentView(batteries);
            stringsResourceLoader = ResourceLoader.GetForCurrentView(resources);
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
            const string fullBattery = "fullBattery";
            const string batteryCharge = "batteryCharge";
            const string battery = "battery";
            const string halfbattery = "halfBattery";
            const string lowBattery = "lowBattery";
            const string emptyBattery = "emptyBattery";
            const string batteryAttention = "batteryAttention";

            await CoreApplication.MainView.CoreWindow.Dispatcher
                .RunAsync(CoreDispatcherPriority.Normal,
            () =>
            {
                var batteryReport = Windows.Devices.Power.Battery.AggregateBattery.GetReport();

                double percentage = (batteryReport.RemainingCapacityInMilliwattHours.Value /
                (double)batteryReport.FullChargeCapacityInMilliwattHours.Value);

                BatteryLevel = percentage * 100;
                BatteryLevelPercentage = $"{(int)BatteryLevel} %";

                switch (batteryReport.Status)
                {
                    case BatteryStatus.Idle:
                        BatteryImage = batteryResourceLoader.GetString(fullBattery);
                        break;
                    case BatteryStatus.Charging:
                        BatteryImage = batteryResourceLoader.GetString(batteryCharge);
                        break;
                    case BatteryStatus.Discharging:
                        if (batteryLevel <= 100 && BatteryLevel > 76)
                        {
                            BatteryImage = batteryResourceLoader.GetString(fullBattery);
                        }
                        else if (BatteryLevel <= 76 && BatteryLevel > 51)
                        {
                            BatteryImage = batteryResourceLoader.GetString(battery);
                        }
                        else if (BatteryLevel <= 51 && BatteryLevel > 31)
                        {
                            BatteryImage = batteryResourceLoader.GetString(halfbattery);
                        }
                        else if (BatteryLevel <= 31 && BatteryLevel > 21)
                        {
                            BatteryImage = batteryResourceLoader.GetString(lowBattery);
                        }
                        else
                        {
                            BatteryImage = batteryResourceLoader.GetString(emptyBattery);
                        }
                        break;
                    default:
                        BatteryImage = batteryResourceLoader.GetString(batteryAttention);
                        break;
                }
            });
        }

        private async Task UpdateMemoryStatus()
        {
            const string memoryUsage = "memoryUsage";
            await CoreApplication.MainView.CoreWindow.Dispatcher
                .RunAsync(CoreDispatcherPriority.Normal,
            () =>
            {
                var usageInB = MemoryManager.AppMemoryUsage;
                var usageInKB = usageInB / 1024.0;
                var usageInMB = usageInKB / 1024.0;

                RamMemoryUsed = stringsResourceLoader.GetString(memoryUsage) + $": {Math.Round(usageInMB, 2)} Mb";
            });
        }

        private async void GetFreeSpace()
        {
            const string freeSpace = "freeSpace";
            const string freeSpaceKey = "System.FreeSpace";
            var retrieveProperties = await ApplicationData.Current.LocalFolder.Properties.RetrievePropertiesAsync(new string[] { freeSpaceKey });
            var freeSpaceRemaining = retrieveProperties[freeSpaceKey];

            var sizeInKB = (ulong)freeSpaceRemaining / 1024.0;
            var sizeInMB = sizeInKB / 1024.0;
            var sizeInGb = sizeInMB / 1024.0;

            FreeSpaceGb = stringsResourceLoader.GetString(freeSpace) + $": {Math.Round(sizeInGb, 2)} Gb";
        }
    }
}
