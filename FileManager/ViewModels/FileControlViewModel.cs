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
        private bool isEditMode;
        private bool isReadOnlyMode;
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
        public bool IsEditMode
        {
            get => isEditMode;
            set
            {
                if (isEditMode != value)
                {
                    isEditMode = value;
                    OnPropertyChanged();
                }
            }
        }
        public bool IsReadOnlyMode
        {
            get => isReadOnlyMode;
            set
            {
                if (isReadOnlyMode != value)
                {
                    isReadOnlyMode = value;
                    OnPropertyChanged();
                }
            }
        }

        public FileControlViewModel()
        {
            DisplayName = "";
            isReadOnlyMode = true;
        }
        public FileControlViewModel(string displayName)
        {
            DisplayName = displayName;
            isReadOnlyMode = true;
        }
    }
}
