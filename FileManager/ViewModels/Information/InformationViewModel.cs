using System.Collections.ObjectModel;
using System.Linq;
using Windows.System;

namespace FileManager.ViewModels.Information
{
    public class InformationViewModel : BindableBase
    {
        private Collection<InformationControlViewModel> informationControls;
        public Collection<InformationControlViewModel> InformationControls
        {
            get => informationControls;
            set
            {
                if (informationControls != value)
                {
                    informationControls = value;
                    OnPropertyChanged();
                }
            }
        }

        public InformationViewModel()
        {
            InformationControls = new Collection<InformationControlViewModel>
            {
                new BatteryControlViewModel(),
                new SpaceControlViewModel(),
                new MemoryControlViewModel()
            };
            BatteryTrigger();
            MemoryTrigger();
        }

        public void BatteryTrigger()
        {
            var batteryControls = InformationControls.Where(c => c.GetType() == typeof(BatteryControlViewModel));
            foreach (var batteryControl in batteryControls)
            {
                Windows.Devices.Power.Battery.AggregateBattery.ReportUpdated
            += async (sender, args) => await batteryControl.UpdateBatteryStatus().ConfigureAwait(true);
            }
        }

        public void MemoryTrigger()
        {
            var memoryControls = InformationControls.Where(c => c.GetType() == typeof(MemoryControlViewModel));
            foreach (var memoryControl in memoryControls)
            {
                MemoryManager.AppMemoryUsageDecreased
                    += async (sender, args) => await memoryControl.UpdateMemoryStatus().ConfigureAwait(true);
                MemoryManager.AppMemoryUsageIncreased
                    += async (sender, args) => await memoryControl.UpdateMemoryStatus().ConfigureAwait(true);
            }
        }
    }
}
