using FileManager.Views;
using Windows.ApplicationModel.Resources;
using Windows.UI.Xaml.Controls;

namespace FileManager.ViewModels
{
    public class MainViewModel : BindableBase
    {
        private const string imageNavigation = "ImageNav";
        private const string videoNavigation = "VideoNav";
        private const string musicNavigation = "MusicNav";
        private const string infoNavigation = "InformationNav";
        private const string mainNavigation = "MainPage";
        private const string googleDriveNav = "GoogleDriveNav";
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
                    SelectionChanged();
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
        public MainViewModel()
        {
            currentContent = new MainTitlePage();
            resourceLoader = ResourceLoader.GetForCurrentView("Resources");
            CurrentTitle = resourceLoader.GetString("MainPage");
        }

        private void SelectionChanged()
        {
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
                case "4":
                    CurrentContent = new GoogleDrivePage();
                    CurrentTitle = resourceLoader.GetString(googleDriveNav);
                    break;
                default:
                    CurrentContent = new MainTitlePage();
                    CurrentTitle = resourceLoader.GetString(mainNavigation);
                    break;
            }
        }
    }
}
