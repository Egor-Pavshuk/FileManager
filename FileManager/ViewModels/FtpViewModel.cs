using FileManager.Commands;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.ApplicationModel.Core;
using Windows.ApplicationModel.Resources;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using Windows.UI;
using Windows.UI.Xaml.Controls;

namespace FileManager.ViewModels
{
    public class FtpViewModel : BindableBase
    {
        private const string protocolName = "ftp://";
        private string hostLink;
        private string username;
        private string password;
        private string currentPath;
        private string loadingText;
        private bool isLoadingVisible;
        private bool isFilesVisible;
        private bool isLoginFormVisible;
        private bool isCommandPanelVisible;
        private FileControlViewModel selectedGridItem;
        private ResourceLoader themeResourceLoader;
        private ResourceLoader stringsResourceLoader;
        private Collection<FileControlViewModel> storageFiles;
        private ICommand connectCommand;
        private ICommand doubleClickedCommand;

        public string HostLink
        {
            get => hostLink;
            set
            {
                if (hostLink != value)
                {
                    hostLink = value;
                    OnPropertyChanged();
                }
            }
        }
        public string Username
        {
            get => username;
            set
            {
                if (username != value)
                {
                    username = value;
                    OnPropertyChanged();
                }
            }
        }
        public string Password
        {
            get => password;
            set
            {
                if (password != value)
                {
                    password = value;
                    OnPropertyChanged();
                }
            }
        }
        public string LoadingText
        {
            get => loadingText;
            set
            {
                if (loadingText != value)
                {
                    loadingText = value;
                    OnPropertyChanged();
                }
            }
        }
        public bool IsLoadingVisible
        {
            get => isLoadingVisible;
            set
            {
                if (isLoadingVisible != value)
                {
                    isLoadingVisible = value;
                    OnPropertyChanged();
                }
            }
        }
        public bool IsFilesVisible
        {
            get => isFilesVisible;
            set
            {
                if (isFilesVisible != value)
                {
                    isFilesVisible = value;
                    OnPropertyChanged();
                }
            }
        }
        public bool IsLoginFormVisible
        {
            get => isLoginFormVisible;
            set
            {
                if (isLoginFormVisible != value)
                {
                    isLoginFormVisible = value;
                    OnPropertyChanged();
                }
            }
        }
        public bool IsCommandPanelVisible
        {
            get => isCommandPanelVisible;
            set
            {
                if (isCommandPanelVisible != value)
                {
                    isCommandPanelVisible = value;
                    OnPropertyChanged();
                }
            }
        }
        public Collection<FileControlViewModel> StorageFiles
        {
            get => storageFiles;
            set
            {
                if (storageFiles != value)
                {
                    storageFiles = value;
                    OnPropertyChanged();
                }
            }
        }
        public FileControlViewModel SelectedGridItem
        {
            get => selectedGridItem;
            set
            {
                if (selectedGridItem != value)
                {
                    selectedGridItem = value;
                    OnPropertyChanged();
                }
            }
        }
        public ICommand ConnectCommand
        {
            get => connectCommand;
            set
            {
                if(connectCommand != value)
                {
                    connectCommand = value;
                    OnPropertyChanged();
                }
            }
        }
        public ICommand DoubleClickedCommand
        {
            get => doubleClickedCommand;
            set
            {
                if (doubleClickedCommand != value)
                {
                    doubleClickedCommand = value;
                    OnPropertyChanged();
                }
            }
        }

        public FtpViewModel()
        {
            const string resources = "Resources";
            const string loading = "loadingText";
            stringsResourceLoader = ResourceLoader.GetForCurrentView(resources);
            LoadingText = stringsResourceLoader.GetString(loading);
            ConnectCommand = new RelayCommand(Connect);
            DoubleClickedCommand = new RelayCommand(OpenFolder);
            HostLink = protocolName + "192.168.0.103:";
            IsLoginFormVisible = true;
            ChangeColorMode(settings, this);
        }
        protected override void ChangeColorMode(UISettings uiSettings, object sender)
        {
            const string image = "image";
            const string video = "video";
            const string audio = "audio";
            const string folder = "folder";
            const string file = "file";
            const string imagesDark = "ImagesDark";
            const string imagesLight = "ImagesLight";

            var currentBackgroundColor = uiSettings?.GetColorValue(UIColorType.Background);
            if (backgroundColor != currentBackgroundColor || storageFiles == null)
            {
                if (currentBackgroundColor == Colors.Black)
                {
                    themeResourceLoader = ResourceLoader.GetForViewIndependentUse(imagesDark);
                    backgroundColor = Colors.Black;
                }
                else
                {
                    themeResourceLoader = ResourceLoader.GetForViewIndependentUse(imagesLight);
                    backgroundColor = Colors.White;
                }

                if (storageFiles != null)
                {
                    CoreApplication.MainView.CoreWindow.Dispatcher
                    .RunAsync(CoreDispatcherPriority.Normal,
                    () =>
                    {
                        foreach (var storageFile in storageFiles)
                        {
                            switch (storageFile.Type)
                            {
                                case image:
                                    storageFile.Image = themeResourceLoader.GetString(image);
                                    break;
                                case video:
                                    storageFile.Image = themeResourceLoader.GetString(video);
                                    break;
                                case audio:
                                    storageFile.Image = themeResourceLoader.GetString(audio);
                                    break;
                                case folder:
                                    storageFile.Image = themeResourceLoader.GetString(folder);
                                    break;
                                default:
                                    storageFile.Image = themeResourceLoader.GetString(file);
                                    break;
                            }
                        }
                    }).AsTask().ConfigureAwait(true);
                }
            }
        }

        private async void Connect(object sender)
        {
            try
            {
                FtpWebRequest request = (FtpWebRequest)WebRequest.Create(hostLink);
                request.Method = WebRequestMethods.Ftp.ListDirectory;
                request.Credentials = new NetworkCredential(username, password);
                var response = (FtpWebResponse)await request.GetResponseAsync().ConfigureAwait(true);
                currentPath = HostLink;
                IsCommandPanelVisible = true;
                IsLoginFormVisible = false;
                _ = GetItemsAsync(currentPath).ConfigureAwait(true);
            }
            catch (Exception)
            {

                throw;
            }           
            
        }

        private void OpenFolder(object sender)
        {
            const string folder = "folder";
            if (selectedGridItem != null && selectedGridItem.Type == folder)
            {
                _ = GetItemsAsync(selectedGridItem.Path);
                currentPath = selectedGridItem.Path;
            }
        }

        private async Task GetParentAsync()
        {
            
        }

        private async Task GetItemsAsync(string path)
        {
            const string folder = "folder";
            const string image = "image";
            const string video = "video";
            const string audio = "audio";
            const string file = "file";
            IsFilesVisible = false;
            IsLoadingVisible = true;

            var ftpFiles = await GetFilesFromFTPAsync(path).ConfigureAwait(true);
            List<FileControlViewModel> items = new List<FileControlViewModel>();

            foreach (var ftpFile in ftpFiles)
            {
                var elements = ftpFile.Split(" ");
                if (elements[0] != "drwxr-xr-x")
                {
                    continue;
                }

                items.Add(new FileControlViewModel()
                {
                    Image = themeResourceLoader.GetString(folder),
                    DisplayName = elements.Last(), //todo another search for name
                    Type = folder,
                    Path = currentPath + "/" + elements.Last()
                });
            }

            foreach (var ftpFile in ftpFiles)
            {
                var elements = ftpFile.Split(" ");
                if (elements[0] == "drwxr-xr-x")
                {
                    continue;
                }

                items.Add(new FileControlViewModel()
                {
                    Image = themeResourceLoader.GetString(file),
                    DisplayName = elements.Last(),
                    Type = file,
                    Path = currentPath + "/" + elements.Last()
                });
            }

            StorageFiles = new Collection<FileControlViewModel>(items);
            IsLoadingVisible = false;
            IsFilesVisible = true;
        }
        private async Task<List<string>> GetFilesFromFTPAsync(string path)
        {
            FtpWebRequest request = (FtpWebRequest)WebRequest.Create(path);
            request.Method = WebRequestMethods.Ftp.ListDirectoryDetails;
            request.Credentials = new NetworkCredential(username, password);

            var response = (FtpWebResponse)await request.GetResponseAsync().ConfigureAwait(true);
            StreamReader streamReader = new StreamReader(response.GetResponseStream());
            List<string> files = new List<string>();

            string line = await streamReader.ReadLineAsync().ConfigureAwait(true);
            while (!string.IsNullOrEmpty(line))
            {
                files.Add(line);
                line = await streamReader.ReadLineAsync().ConfigureAwait(true);
            }
            streamReader.Close();

            return files;
        }
    }
}
