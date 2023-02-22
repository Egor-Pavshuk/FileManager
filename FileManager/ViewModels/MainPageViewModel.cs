using FileManager.Views;
using Windows.ApplicationModel.Resources;
using Windows.UI.Xaml.Controls;

namespace FileManager.ViewModels
{
    public class MainPageViewModel : BindableBase
    {
        private const string imageNavigation = "ImageNav";
        private const string videoNavigation = "VideoNav";
        private const string musicNavigation = "MusicNav";
        private const string infoNavigation = "InformationNav";
        private const string mainNavigation = "MainPage";
        private Page currentContent;
        private string currentTitle;
        private ResourceLoader resourceLoader;
        private NavigationViewItem selectedItem;
        public Page CurrentContent
        {
            get => currentContent;
            set
            {
                if (currentContent != value)
                {
                    currentContent = value;
                    OnPropertyChanged();
                }
            }
        }
        public NavigationViewItem SelectedItem
        {
            get => selectedItem;
            set
            {
                if (selectedItem != value)
                {
                    selectedItem = value;
                    OnPropertyChanged();
                }
            }
        }
        public string CurrentTitle
        {
            get => currentTitle;
            set
            {
                if (currentTitle != value)
                {
                    currentTitle = value;
                    OnPropertyChanged();
                }
            }
        }
        public MainPageViewModel()
        {
            currentContent = new MainPageTitle();
            resourceLoader = ResourceLoader.GetForCurrentView("Resources");
            CurrentTitle = resourceLoader.GetString("MainPage");
        }
        public void SelectionChanged(object sender, NavigationViewSelectionChangedEventArgs e)
        {
            SelectedItem = (NavigationViewItem)e?.SelectedItem;
            switch (selectedItem.AccessKey)
            {
                case "0":
                    CurrentContent = new PicturesLibraryPage();
                    CurrentTitle = resourceLoader.GetString(imageNavigation);
                    break;
                case "1":
                    CurrentContent = new VideosLibraryPage();
                    CurrentTitle = resourceLoader.GetString(videoNavigation);
                    break;
                case "2":
                    CurrentContent = new MusicsLibraryPage();
                    CurrentTitle = resourceLoader.GetString(musicNavigation);
                    break;
                case "3":
                    CurrentContent = new InformationPage();
                    CurrentTitle = resourceLoader.GetString(infoNavigation);
                    break;
                default:
                    CurrentContent = new MainPageTitle();
                    CurrentTitle = resourceLoader.GetString(mainNavigation);
                    break;
            }
        }
    }
}
