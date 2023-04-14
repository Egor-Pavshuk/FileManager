using Windows.ApplicationModel.Resources;
using FileManager.Helpers;

namespace FileManager.ViewModels
{
    public class MainTitleViewModel : BindableBase
    {
        private readonly ResourceLoader resourceLoader;
        private string title;
        public string Title
        {
            get => title;
            set
            {
                if (title != value)
                {
                    title = value;
                    OnPropertyChanged();
                }
            }
        }

        public MainTitleViewModel()
        {
            resourceLoader = ResourceLoader.GetForCurrentView(Constants.StringResources);
            Title = resourceLoader.GetString(Constants.WelcomeTitle);
        }
    }
}
