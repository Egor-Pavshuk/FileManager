using FileManager.Views;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Windows.UI.Xaml.Controls;

namespace FileManager.ViewModels
{
    public class MainPageViewModel : INotifyPropertyChanged
    {
        private Page currentContent;
        private string currentTitle;
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
            CurrentTitle = "Main page";
        }
        public void SelectionChanged(object sender, NavigationViewSelectionChangedEventArgs e)
        {
            SelectedItem = (NavigationViewItem)e.SelectedItem;
            switch (selectedItem.AccessKey)
            {
                case "0":
                    CurrentContent = new PicturesLibraryPage();
                    CurrentTitle = "Images";
                    break;
                case "1":
                    CurrentContent = new VideosLibraryPage();
                    CurrentTitle = "Videos";
                    break;
                default:
                    CurrentContent = new Page();
                    CurrentTitle = "Main page";
                    break;
            }
        }
    }
}
