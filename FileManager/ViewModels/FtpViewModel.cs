﻿using FileManager.Commands;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.ApplicationModel.Core;
using Windows.ApplicationModel.Resources;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Popups;
using Windows.UI.ViewManagement;

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
        private bool isBackButtonAvailable;
        private List<string> downloadingFilesPath;
        private Dictionary<string, string> knownTypes;
        private OnlineFileControlViewModel selectedGridItem;
        private ResourceLoader themeResourceLoader;
        private ResourceLoader stringsResourceLoader;
        private Collection<OnlineFileControlViewModel> storageFiles;
        private ICommand connectCommand;
        private ICommand doubleClickedCommand;
        private ICommand getParentCommand;
        private ICommand downloadFileCommand;

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
        public bool IsBackButtonAvailable
        {
            get => isBackButtonAvailable;
            set
            {
                if (isBackButtonAvailable != value)
                {
                    isBackButtonAvailable = value;
                    OnPropertyChanged();
                }
            }
        }
        public Collection<OnlineFileControlViewModel> StorageFiles
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
        public OnlineFileControlViewModel SelectedGridItem
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
                if (connectCommand != value)
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
        public ICommand GetParentCommand
        {
            get => getParentCommand;
            set
            {
                if (getParentCommand != value)
                {
                    getParentCommand = value;
                    OnPropertyChanged();
                }
            }
        }
        public ICommand DownloadFileCommand
        {
            get => downloadFileCommand;
            set
            {
                if (downloadFileCommand != value)
                {
                    downloadFileCommand = value;
                    OnPropertyChanged();
                }
            }
        }


        public FtpViewModel()
        {
            const string resources = "Resources";
            const string loading = "loadingText";
            const string image = "image";
            const string video = "video";
            const string audio = "audio";
            knownTypes = new Dictionary<string, string>()
            {
                { ".jpg", image },
                { ".jpeg", image },
                { ".jfif", image },
                { ".gif", image },
                { ".png", image },
                { ".mp4", video },
                { ".avi", video },
                { ".wmv", video },
                { ".amv", video },
                { ".mp3", audio },
                { ".ogg", audio },
                { ".wma", audio },
                { ".wav", audio },
            };
            downloadingFilesPath = new List<string>();
            stringsResourceLoader = ResourceLoader.GetForCurrentView(resources);
            LoadingText = stringsResourceLoader.GetString(loading);
            ConnectCommand = new RelayCommand(ConnectAsync);
            DoubleClickedCommand = new RelayCommand(OpenFolder);
            GetParentCommand = new RelayCommand(GetParentAsync);
            DownloadFileCommand = new RelayCommand(DownloadFileAsync);
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

        private async void ConnectAsync(object sender)
        {
            const string connectionError = "connectionError";
            const string connectionErrorContent = "connectionErrorContent";

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
            catch (WebException)
            {
                await new MessageDialog(stringsResourceLoader.GetString(connectionErrorContent))
                {
                    Title = stringsResourceLoader.GetString(connectionError)
                }.ShowAsync();
            }

        }

        private void OpenFolder(object sender)
        {
            const string folder = "folder";
            if (selectedGridItem != null && selectedGridItem.Type == folder)
            {
                IsBackButtonAvailable = true;
                _ = GetItemsAsync(selectedGridItem.Path);
                currentPath = selectedGridItem.Path;
            }
        }

        private void GetParentAsync(object sender)
        {
            currentPath = currentPath.Substring(0, currentPath.LastIndexOf('/'));
            if (currentPath == hostLink)
            {
                IsBackButtonAvailable = false;
            }
            _ = GetItemsAsync(currentPath).ConfigureAwait(true);
        }

        private async Task GetItemsAsync(string path)
        {
            const string folder = "folder";
            const string file = "file";
            IsFilesVisible = false;
            IsLoadingVisible = true;

            var ftpFiles = await GetFilesFromFTPAsync(path).ConfigureAwait(true);
            List<OnlineFileControlViewModel> items = new List<OnlineFileControlViewModel>();

            foreach (var ftpFile in ftpFiles)
            {
                var elements = ftpFile.Split(" ").ToList();
                int indexOfName = elements.IndexOf(elements.FirstOrDefault(e => Regex.IsMatch(e, @"[0-2][0-9][:][0-5][0-9]"))) + 1;
                var elementName = string.Join(" ", elements.GetRange(indexOfName, elements.Count - indexOfName));
                if (elements[0] != "drwxr-xr-x")
                {
                    continue;
                }

                items.Add(new OnlineFileControlViewModel()
                {
                    Image = themeResourceLoader.GetString(folder),
                    DisplayName = elementName,
                    Type = folder,
                    Path = currentPath + "/" + elementName
                });
            }

            foreach (var ftpFile in ftpFiles)
            {
                var elements = ftpFile.Split(" ").ToList();
                int indexOfName = elements.IndexOf(elements.FirstOrDefault(e => Regex.IsMatch(e, @"[0-2][0-9][:][0-5][0-9]"))) + 1;
                var elementName = string.Join(" ", elements.GetRange(indexOfName, elements.Count - indexOfName));
                if (elements[0] == "drwxr-xr-x")
                {
                    continue;
                }

                int startIndexOfExtension = elementName.LastIndexOf('.');
                string fileExtension = string.Empty;
                if (startIndexOfExtension >= 0)
                {
                    fileExtension = elementName.Substring(startIndexOfExtension);
                }
                if (knownTypes.TryGetValue(fileExtension, out string value))
                {
                    items.Add(new OnlineFileControlViewModel()
                    {
                        //Id = Guid.NewGuid().ToString(),
                        Image = themeResourceLoader.GetString(value),
                        DisplayName = elementName,
                        Type = file,
                        Path = currentPath + "/" + elementName
                    });
                }
                else
                {
                    items.Add(new OnlineFileControlViewModel()
                    {
                        //Id = Guid.NewGuid().ToString(),
                        Image = themeResourceLoader.GetString(file),
                        DisplayName = elementName,
                        Type = file,
                        Path = currentPath + "/" + elementName
                    });
                }

            }

            StorageFiles = new Collection<OnlineFileControlViewModel>(items);
            CheckFilesForDownloadingAsync();
            IsLoadingVisible = false;
            IsFilesVisible = true;
        }

        private async void CheckFilesForDownloadingAsync()
        {
            const string downloadingText = "downloadingText";
            const string folder = "folder";
            await Task.Run(() =>
            {
                foreach (var file in storageFiles)
                {
                    if (file.Type == folder)
                    {
                        continue;
                    }
                    if (downloadingFilesPath.Exists(p => p == file.Path))
                    {
                        file.IsDownloading = true;
                        file.DownloadStatus = stringsResourceLoader.GetString(downloadingText);
                    }
                }
            }).ConfigureAwait(true);            
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

        private async void DownloadFileAsync(object sender)
        {
            const string folder = "folder";
            const string downloadingText = "downloadingText";
            if (selectedGridItem != null && !string.IsNullOrEmpty(selectedGridItem.DisplayName) && selectedGridItem.Type != folder)
            {
                var picker = new FolderPicker
                {
                    ViewMode = PickerViewMode.Thumbnail,
                    SuggestedStartLocation = PickerLocationId.Downloads
                };
                picker.FileTypeFilter.Add("*");
                StorageFolder downloadFolder = await picker.PickSingleFolderAsync();

                if (downloadFolder != null)
                {
                    StorageFile destinationFile = await downloadFolder.CreateFileAsync(
                        selectedGridItem.DisplayName, CreationCollisionOption.GenerateUniqueName);

                    SelectedGridItem.IsDownloading = true;
                    SelectedGridItem.DownloadStatus = stringsResourceLoader.GetString(downloadingText);
                    downloadingFilesPath.Add(selectedGridItem.Path);

                    DownloadFtpFileAsync(destinationFile, selectedGridItem.Path);
                }
            }
        }

        private async void DownloadFtpFileAsync(StorageFile destinationFile, string filePath)
        {
            const string downloadCompleted = "downloadCompleted";
            const string connectionError = "connectionError";
            const string connectionErrorContent = "connectionErrorContent";
            const string failed = "failed";
            bool isDownloadSuccess;

            Stream stream;
            try
            {
                FtpWebRequest request = (FtpWebRequest)WebRequest.Create(filePath);
                request.Method = WebRequestMethods.Ftp.DownloadFile;
                request.Credentials = new NetworkCredential(username, password);
                var response = (FtpWebResponse)await request.GetResponseAsync().ConfigureAwait(true);

                stream = response.GetResponseStream();
                using (var fileStream = await destinationFile.OpenStreamForWriteAsync().ConfigureAwait(true))
                {
                    await stream.CopyToAsync(fileStream).ConfigureAwait(true);
                }
                isDownloadSuccess = true;
            }
            catch (WebException)
            {
                await new MessageDialog(stringsResourceLoader.GetString(connectionErrorContent))
                {
                    Title = stringsResourceLoader.GetString(connectionError)
                }.ShowAsync();
                isDownloadSuccess = false;
                await destinationFile.DeleteAsync();
            }

            downloadingFilesPath.Remove(filePath);
            var downloadingFile = storageFiles.FirstOrDefault(f => f.Path == filePath);
            if (downloadingFile != null)
            {
                if (isDownloadSuccess)
                {
                    downloadingFile.DownloadStatus = stringsResourceLoader.GetString(downloadCompleted);
                }
                else
                {
                    downloadingFile.DownloadStatus = stringsResourceLoader.GetString(failed);
                }
                CloseDownloadingAsync(downloadingFile);
            }
        }

        private async void CloseDownloadingAsync(OnlineFileControlViewModel file)
        {
            await Task.Delay(3000).ConfigureAwait(true);
            file.IsDownloading = false;
            file.DownloadStatus = string.Empty;
        }
    }
}
