using System.Threading.Tasks;
using Windows.ApplicationModel.Resources;
using FileManager.Helpers;

namespace FileManager.ViewModels.Information
{
    public class InformationControlViewModel : BindableBase
    {
        protected readonly ResourceLoader stringsResourceLoader;
        protected readonly ResourceLoader batteryResourceLoader;
        protected readonly ResourceLoader themeResourceLoader;
        private bool isProgressBarVisible;
        private double progressBarValue;
        private string background;
        private string image;
        private string text;

        public string Background
        {
            get => background;
            set
            {
                if (background != value)
                {
                    background = value;
                    OnPropertyChanged();
                }
            }
        }
        public string Image
        {
            get => image;
            set
            {
                if (image != value)
                {
                    image = value;
                    OnPropertyChanged();
                }
            }
        }
        public double ProgressBarValue
        {
            get => progressBarValue;
            set
            {
                if (progressBarValue != value)
                {
                    progressBarValue = value;
                    OnPropertyChanged();
                }
            }
        }
        public bool IsProgressBarVisible
        {
            get => isProgressBarVisible;
            set
            {
                if (isProgressBarVisible != value)
                {
                    isProgressBarVisible = value;
                    OnPropertyChanged();
                }
            }
        }
        public string Text
        {
            get => text;
            set
            {
                if (text != value)
                {
                    text = value;
                    OnPropertyChanged();
                }
            }
        }
        public InformationControlViewModel()
        {
            batteryResourceLoader = ResourceLoader.GetForCurrentView(Constants.Batteries);
            stringsResourceLoader = ResourceLoader.GetForCurrentView(Constants.Resources);
            themeResourceLoader = ResourceLoader.GetForCurrentView(Constants.ImagesLight);
        }

        public virtual Task UpdateBatteryStatus() => null;
        public virtual Task UpdateMemoryStatus() => null;
        public virtual Task GetFreeSpaceAsync() => null;
    }
}
