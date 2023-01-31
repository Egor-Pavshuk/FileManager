using FileManager.Views;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;

namespace FileManager.ViewModels
{
    public class MainPageViewModel : INotifyPropertyChanged
    {
        private Page currentContent;
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
        public MainPageViewModel()
        {
            currentContent = new Page();
        }
        public void SelectionChanged(object sender, NavigationViewSelectionChangedEventArgs e)
        {
            SelectedItem = (NavigationViewItem)e.SelectedItem;
            switch (selectedItem.AccessKey)
            {
                case "0":
                    CurrentContent = new DirectoryFilesPage();
                    break;
                default:
                    CurrentContent = new Page();
                    break;
            }
        }
    }
}
