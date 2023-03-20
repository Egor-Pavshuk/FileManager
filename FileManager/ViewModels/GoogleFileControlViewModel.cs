using System.IO;
using System.Threading.Tasks;
using Windows.Networking.BackgroundTransfer;

namespace FileManager.ViewModels
{
    public class GoogleFileControlViewModel : BindableBase
    {
        private string id;
        private string image;
        private string displayName;
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
        public GoogleFileControlViewModel()
        {
            DisplayName = "";
        }
        public GoogleFileControlViewModel(string displayName)
        {
            DisplayName = displayName;
        }
    }
}
