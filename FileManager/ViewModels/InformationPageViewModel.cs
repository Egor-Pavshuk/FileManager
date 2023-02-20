using System;
using System.Collections.Generic;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Runtime;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.ApplicationModel.Resources;
using Windows.Media.SpeechSynthesis;
using Windows.Storage;
using Windows.System.Power;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.ViewManagement;

namespace FileManager.ViewModels
{
    public class InformationPageViewModel : BindableBase
    {
        private string batteryImage;
        private double batteryLevel;
        private string freeSpaceGb;
        private string batteryLevelPercentage;
        private ResourceLoader resourceLoader;
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
        public InformationPageViewModel()
        {
            //ChangeColorMode(settings, this);
            resourceLoader = ResourceLoader.GetForCurrentView("Batteries");
            BatteryTrigger();
            UpdateBatteryStatus().ConfigureAwait(true);
            GetFreeSpace();
        }

        public void BatteryTrigger()
        {
            Windows.Devices.Power.Battery.AggregateBattery.ReportUpdated
            += async (sender, args) => await UpdateBatteryStatus().ConfigureAwait(true);            
        }
        //protected override void ChangeColorMode(UISettings settings, object sender)
        //{
        //    {
        //        var currentBackgroundColor = settings?.GetColorValue(UIColorType.Background);
        //        if (backgroundColor == currentBackgroundColor)
        //        {
        //            return;
        //        }

        //        if (currentBackgroundColor == Colors.Black)
        //        {
        //            resourceLoader = ResourceLoader.GetForViewIndependentUse("ImagesDark");
        //            backgroundColor = Colors.Black;
        //        }
        //        else
        //        {
        //            resourceLoader = ResourceLoader.GetForViewIndependentUse("ImagesLight");
        //            backgroundColor = Colors.White;
        //        }

        //        if (storageFiles is null)
        //        {
        //            return;
        //        }
        //        CoreApplication.MainView.CoreWindow.Dispatcher
        //            .RunAsync(CoreDispatcherPriority.Normal,
        //            () =>
        //            {
        //                foreach (var file in storageFiles)
        //                {
        //                    if (file.Type == "File")
        //                    {
        //                        file.Image = resourceLoader.GetString("File");
        //                    }
        //                    else
        //                    {
        //                        file.Image = resourceLoader.GetString("Folder");
        //                    }
        //                }
        //            }).AsTask().ConfigureAwait(true);
        //    }
        //}
        private async Task UpdateBatteryStatus()
        {
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
                        BatteryImage = resourceLoader.GetString("fullbattery");
                        break;
                    case BatteryStatus.Charging:
                        BatteryImage = resourceLoader.GetString("batteryCharge");
                        break;
                    case BatteryStatus.Discharging:
                        if (batteryLevel <= 100 && BatteryLevel > 76)
                        {
                            BatteryImage = resourceLoader.GetString("fullbattery");
                        }
                        else if (BatteryLevel <= 76 && BatteryLevel > 51)
                        {
                            BatteryImage = resourceLoader.GetString("battery");
                        }
                        else if (BatteryLevel <= 51 && BatteryLevel > 31)
                        {
                            BatteryImage = resourceLoader.GetString("halfBattery");
                        }
                        else if (BatteryLevel <= 31 && BatteryLevel > 21)
                        {
                            BatteryImage = resourceLoader.GetString("lowBattery");
                        }
                        else
                        {
                            BatteryImage = resourceLoader.GetString("emptyBattery");
                        }
                        break;
                    default:
                        BatteryImage = resourceLoader.GetString("batteryAttention");
                        break;
                }
            });
        }
        private async void GetFreeSpace()
        {
            string freeSpaceKey = "System.FreeSpace";
            var retrieveProperties = await ApplicationData.Current.LocalFolder.Properties.RetrievePropertiesAsync(new string[] { freeSpaceKey });
            var freeSpaceRemaining = retrieveProperties[freeSpaceKey];

            var sizeInKB = (ulong)freeSpaceRemaining / 1024.0;
            var sizeInMB = sizeInKB / 1024.0;
            var sizeInGb = sizeInMB / 1024.0;

            FreeSpaceGb = $"Free space: {Math.Round(sizeInGb, 2)} Gb";
        }
    }
}
