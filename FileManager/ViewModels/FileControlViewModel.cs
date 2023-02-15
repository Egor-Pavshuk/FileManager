using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace FileManager.ViewModels
{
    public class FileControlViewModel : BindableBase
    {
        private string image;
        private string displayName;
        private string path;
        private string type;
        public string Image
        {
            get { return image; }
            set
            {
                if (image != value)
                {
                    image = value;
                    OnPropertyChanged();
                }
            }
        }
        public string DisplayName
        {
            get { return displayName; }
            set
            {
                if (displayName != value)
                {
                    displayName = value;
                    OnPropertyChanged();
                }
            }
        }
        public string Path
        {
            get { return path; }
            set
            {
                if (path != value)
                {
                    path = value;
                    OnPropertyChanged();
                }
            }
        }
        public string Type
        {
            get { return type; }
            set
            {
                if (type != value)
                {
                    type = value;
                    OnPropertyChanged();
                }
            }
        }
        public FileControlViewModel()
        {
            DisplayName = "";
        }
        public FileControlViewModel(string displayName)
        {
            DisplayName = displayName;
        }
    }
}
