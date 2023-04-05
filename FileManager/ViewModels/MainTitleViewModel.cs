using Windows.ApplicationModel.Resources;

namespace FileManager.ViewModels
{
    public class MainTitleViewModel : BindableBase
    {
        private ResourceLoader resourceLoader;
        private const string Resourses = "Resources";
        private const string WelcomeTitle = "welcomeTitle";
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
            resourceLoader = ResourceLoader.GetForCurrentView(Resourses);
            Title = resourceLoader.GetString(WelcomeTitle);
        }
    }
}
