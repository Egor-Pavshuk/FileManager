using System;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.System.Power;
using Windows.UI.Core;
using FileManager.Helpers;

namespace FileManager.ViewModels.Information
{
    public class BatteryControlViewModel : InformationControlViewModel
    {
        public BatteryControlViewModel()
        {
            Background = "#FFF4B717";
            IsProgressBarVisible = true;
            UpdateBatteryStatus().ConfigureAwait(true);
        }

        public override async Task UpdateBatteryStatus()
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

                ProgressBarValue = percentage * 100;
                Text = $"{(int)ProgressBarValue} %";

                switch (batteryReport.Status)
                {
                    case BatteryStatus.Idle:
                        Image = batteryResourceLoader.GetString(Constants.FullBattery);
                        break;
                    case BatteryStatus.Charging:
                        Image = batteryResourceLoader.GetString(Constants.BatteryCharge);
                        break;
                    case BatteryStatus.Discharging:
                        if (ProgressBarValue <= 100 && ProgressBarValue > 76)
                        {
                            Image = batteryResourceLoader.GetString(Constants.FullBattery);
                        }
                        else if (ProgressBarValue <= 76 && ProgressBarValue > 51)
                        {
                            Image = batteryResourceLoader.GetString(Constants.Battery);
                        }
                        else if (ProgressBarValue <= 51 && ProgressBarValue > 31)
                        {
                            Image = batteryResourceLoader.GetString(Constants.Halfbattery);
                        }
                        else if (ProgressBarValue <= 31 && ProgressBarValue > 21)
                        {
                            Image = batteryResourceLoader.GetString(Constants.LowBattery);
                        }
                        else
                        {
                            Image = batteryResourceLoader.GetString(Constants.EmptyBattery);
                        }
                        break;
                    default:
                        Image = batteryResourceLoader.GetString(Constants.BatteryAttention);
                        break;
                }
            });
        }
    }
}
