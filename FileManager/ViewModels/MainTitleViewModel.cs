using FileManager.Helpers;
using Windows.ApplicationModel.Resources;

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
            resourceLoader = ResourceLoader.GetForCurrentView(Constants.Resourses);
            Title = resourceLoader.GetString(Constants.WelcomeTitle);
        }
    }
}
