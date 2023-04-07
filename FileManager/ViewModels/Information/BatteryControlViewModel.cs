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
        private const int FullBattery = 100;
        private const int BitDischargedBattery = 76;
        private const int HalfBattery = 51;
        private const int ThirdBattery = 31;
        private const int LowBattery = 21;
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
                        if (ProgressBarValue <= FullBattery && ProgressBarValue > BitDischargedBattery)
                        {
                            Image = batteryResourceLoader.GetString(Constants.FullBattery);
                        }
                        else if (ProgressBarValue <= BitDischargedBattery && ProgressBarValue > HalfBattery)
                        {
                            Image = batteryResourceLoader.GetString(Constants.Battery);
                        }
                        else if (ProgressBarValue <= HalfBattery && ProgressBarValue > ThirdBattery)
                        {
                            Image = batteryResourceLoader.GetString(Constants.Halfbattery);
                        }
                        else if (ProgressBarValue <= ThirdBattery && ProgressBarValue > LowBattery)
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
