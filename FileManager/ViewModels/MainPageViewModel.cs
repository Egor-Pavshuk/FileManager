using FileManager.Views;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Windows.ApplicationModel.Resources;
using Windows.UI.Xaml.Controls;

namespace FileManager.ViewModels
{
    public class MainPageViewModel : INotifyPropertyChanged
    {
        private Page currentContent;
        private string currentTitle;
        private ResourceLoader resourceLoader;
        private NavigationViewItem selectedItem;
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string prop = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(prop));
            }
        }
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
            currentContent = new Page();
            resourceLoader = Windows.ApplicationModel.Resources.ResourceLoader.GetForCurrentView("Resources");
            CurrentTitle = resourceLoader.GetString("MainPage");
        }
        public void SelectionChanged(object sender, NavigationViewSelectionChangedEventArgs e)
        {
            SelectedItem = (NavigationViewItem)e.SelectedItem;
            switch (selectedItem.AccessKey)
            {
                case "0":
                    CurrentContent = new PicturesLibraryPage();
                    CurrentTitle = resourceLoader.GetString("ImageNav");
                    break;
                case "1":
                    CurrentContent = new VideosLibraryPage();
                    CurrentTitle = resourceLoader.GetString("VideoNav");
                    break;
                case "2":
                    CurrentContent = new MusicsLibraryPage();
                    CurrentTitle = resourceLoader.GetString("MusicNav");
                    break;
                default:
                    CurrentContent = new Page();
                    CurrentTitle = resourceLoader.GetString("MainPage");
                    break;
            }
        }
    }
}
