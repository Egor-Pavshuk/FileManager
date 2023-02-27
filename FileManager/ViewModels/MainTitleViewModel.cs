using Windows.ApplicationModel.Resources;

namespace FileManager.ViewModels
{
    public class MainTitleViewModel : BindableBase
    {
        private ResourceLoader resourceLoader;
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
            resourceLoader = ResourceLoader.GetForCurrentView("Resources");
            Title = resourceLoader.GetString("welcomeTitle");
        }
    }
}
