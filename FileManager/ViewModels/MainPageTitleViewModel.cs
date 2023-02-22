using Windows.ApplicationModel.Resources;

namespace FileManager.ViewModels
{
    public class MainPageTitleViewModel : BindableBase
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
        public MainPageTitleViewModel()
        {
            resourceLoader = ResourceLoader.GetForCurrentView("Resources");
            Title = resourceLoader.GetString("welcomeTitle");
        }
    }
}
