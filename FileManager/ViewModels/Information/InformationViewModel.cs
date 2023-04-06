using System.Collections.ObjectModel;
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
            Windows.Devices.Power.Battery.AggregateBattery.ReportUpdated
            += async (sender, args) => await InformationControls[0].UpdateBatteryStatus().ConfigureAwait(true);
        }

        public void MemoryTrigger()
        {
            MemoryManager.AppMemoryUsageDecreased
            += async (sender, args) => await InformationControls[1].UpdateMemoryStatus().ConfigureAwait(true);
            MemoryManager.AppMemoryUsageIncreased
            += async (sender, args) => await InformationControls[1].UpdateMemoryStatus().ConfigureAwait(true);
        }


    }
}
