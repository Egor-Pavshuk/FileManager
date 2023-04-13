using Windows.ApplicationModel.Resources;

namespace FileManager.ViewModels.OnlineFileControls
{
    public class OnlineFileControlViewModel : BindableBase
    {
        private string id;
        private string image;
        private string displayName;
        private string path;
        private string type;
        private bool isDownloading;
        private int downloadProgress;
        private string downloadStatus;
        public string Id
        {
            get { return id; }
            set
            {
                if (id != value)
                {
                    id = value;
                    OnPropertyChanged();
                }
            }
        }
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
        public string DownloadStatus
        {
            get => downloadStatus;
            set
            {
                if (downloadStatus != value)
                {
                    downloadStatus = value;
                    OnPropertyChanged();
                }
            }
        }
        public bool IsDownloading
        {
            get => isDownloading;
            set
            {
                if (isDownloading != value)
                {
                    isDownloading = value;
                    OnPropertyChanged();
                }
            }
        }
        public int DownloadProgress
        {
            get => downloadProgress;
            set
            {
                if (downloadProgress != value)
                {
                    downloadProgress = value;
                    OnPropertyChanged();
                }
            }
        }

        public OnlineFileControlViewModel()
        {
            DisplayName = "";
        }
        public OnlineFileControlViewModel(string displayName)
        {
            DisplayName = displayName;
        }
        public virtual void ChangeColorMode(ResourceLoader themeResourceLoader) { }
    }
}
