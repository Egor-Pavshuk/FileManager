using FileManager.Commands;
using FileManager.Controlls;
using FileManager.Validation;
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
using Windows.UI.Xaml.Controls;

namespace FileManager.ViewModels
{
    public class FtpViewModel : BindableBase
    {
        private const string ProtocolName = "ftp://";
        private const string Resources = "Resources";
        private const string Loading = "loadingText";
        private const string Image = "image";
        private const string Video = "video";
        private const string Audio = "audio";
        private const string Folder = "folder";
        private const string File = "file";
        private const string ImagesDark = "ImagesDark";
        private const string ImagesLight = "ImagesLight";
        private const string ConnectionError = "connectionError";
        private const string ConnectionErrorContent = "connectionErrorContent";
        private const string Failed = "failed";
        private const string InvalidUriFormat = "invalidUriFormat";
        private const string DownloadingText = "downloadingText";
        private const string DownloadCompleted = "downloadCompleted";
        private const string SameNameError = "sameNameError";
        private const string InputError = "inputError";
        private const string InvalidInput = "invalidInput";
        private const string CancelButton = "cancelButton";
        private const string CreateButton = "createButton";
        private const string NewFolder = "newFolder";
        private const string PlaceHolderFileName = "placeHolderFileName";
        private const string Confirmation = "confirmation";
        private const string DeleteConfirmText = "deleteConfirmText";
        private const string YesButton = "yesButton";
        private const string Rename = "rename";
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
        private readonly List<string> downloadingFilesPath;
        private readonly Dictionary<string, string> knownTypes;
        private OnlineFileControlViewModel selectedGridItem;
        private ResourceLoader themeResourceLoader;
        private readonly ResourceLoader stringsResourceLoader;
        private Collection<OnlineFileControlViewModel> storageFiles;
        private ICommand connectCommand;
        private ICommand doubleClickedCommand;
        private ICommand getParentCommand;
        private ICommand downloadFileCommand;
        private ICommand uploadFileCommand;
        private ICommand deleteFileCommand;
        private ICommand createFolderCommand;
        private ICommand renameFileCommand;
        private ICommand itemClickedCommand;

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
        public ICommand UploadFileCommand
        {
            get => uploadFileCommand;
            set
            {
                if (uploadFileCommand != value)
                {
                    uploadFileCommand = value;
                    OnPropertyChanged();
                }
            }
        }
        public ICommand DeleteFileCommand
        {
            get => deleteFileCommand;
            set
            {
                if (deleteFileCommand != value)
                {
                    deleteFileCommand = value;
                    OnPropertyChanged();
                }
            }
        }
        public ICommand CreateNewFolderCommand
        {
            get => createFolderCommand;
            set
            {
                if (createFolderCommand != value)
                {
                    createFolderCommand = value;
                    OnPropertyChanged();
                }
            }
        }
        public ICommand RenameFileCommand
        {
            get => renameFileCommand;
            set
            {
                if (renameFileCommand != value)
                {
                    renameFileCommand = value;
                    OnPropertyChanged();
                }
            }
        }
        public ICommand ItemClickedCommand
        {
            get => itemClickedCommand;
            set
            {
                if (itemClickedCommand != value)
                {
                    itemClickedCommand = value;
                    OnPropertyChanged();
                }
            }
        }


        public FtpViewModel()
        {            
            knownTypes = new Dictionary<string, string>()
            {
                { ".jpg", Image },
                { ".jpeg", Image },
                { ".jfif", Image },
                { ".gif", Image },
                { ".png", Image },
                { ".mp4", Video },
                { ".avi", Video },
                { ".wmv", Video },
                { ".amv", Video },
                { ".mp3", Audio },
                { ".ogg", Audio },
                { ".wma", Audio },
                { ".wav", Audio },
            };
            downloadingFilesPath = new List<string>();
            stringsResourceLoader = ResourceLoader.GetForCurrentView(Resources);
            LoadingText = stringsResourceLoader.GetString(Loading);
            if (Windows.System.Profile.AnalyticsInfo.VersionInfo.DeviceFamily == "Windows.Xbox")
            {
                ItemClickedCommand = new RelayCommand(OpenFolder);
                DoubleClickedCommand = new RelayCommand((o) => { });
            }
            else
            {
                DoubleClickedCommand = new RelayCommand(OpenFolder);
                ItemClickedCommand = new RelayCommand((o) => { });
            }
            ConnectCommand = new RelayCommand(ConnectAsync);
            DoubleClickedCommand = new RelayCommand(OpenFolder);
            GetParentCommand = new RelayCommand(GetParent);
            DownloadFileCommand = new RelayCommand(DownloadFileAsync);
            UploadFileCommand = new RelayCommand(UploadFileAsync);
            DeleteFileCommand = new RelayCommand(DeleteFileAsync);
            CreateNewFolderCommand = new RelayCommand(CreateNewFolderAsync);
            RenameFileCommand = new RelayCommand(RenameFileAsync);
            HostLink = ProtocolName;
            IsLoginFormVisible = true;
            ChangeColorMode(settings, this);
        }
        protected override void ChangeColorMode(UISettings uiSettings, object sender)
        {
            var currentBackgroundColor = uiSettings?.GetColorValue(UIColorType.Background);
            if (backgroundColor != currentBackgroundColor || storageFiles == null)
            {
                if (currentBackgroundColor == Colors.Black)
                {
                    themeResourceLoader = ResourceLoader.GetForViewIndependentUse(ImagesDark);
                    backgroundColor = Colors.Black;
                }
                else
                {
                    themeResourceLoader = ResourceLoader.GetForViewIndependentUse(ImagesLight);
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
                                case Image:
                                    storageFile.Image = themeResourceLoader.GetString(Image);
                                    break;
                                case Video:
                                    storageFile.Image = themeResourceLoader.GetString(Video);
                                    break;
                                case Audio:
                                    storageFile.Image = themeResourceLoader.GetString(Audio);
                                    break;
                                case Folder:
                                    storageFile.Image = themeResourceLoader.GetString(Folder);
                                    break;
                                default:
                                    storageFile.Image = themeResourceLoader.GetString(File);
                                    break;
                            }
                        }
                    }).AsTask().ConfigureAwait(true);
                }
            }
        }

        private async void ConnectAsync(object sender)
        {           
            try
            {                
                IsLoginFormVisible = false;
                IsLoadingVisible = true;

                FtpWebRequest request = (FtpWebRequest)WebRequest.Create(hostLink);
                request.Method = WebRequestMethods.Ftp.ListDirectory;
                request.Credentials = new NetworkCredential(username, password);
                var response = (FtpWebResponse)await request.GetResponseAsync().ConfigureAwait(true);
                response.Close();
                currentPath = HostLink;

                IsCommandPanelVisible = true;
                _ = GetItemsAsync(currentPath).ConfigureAwait(true);
            }
            catch (WebException)
            {
                await new MessageDialog(stringsResourceLoader.GetString(ConnectionErrorContent))
                {
                    Title = stringsResourceLoader.GetString(ConnectionError)
                }.ShowAsync();

                IsLoadingVisible = false;
                IsLoginFormVisible = true;
            }
            catch (UriFormatException)
            {
                await new MessageDialog(stringsResourceLoader.GetString(InvalidUriFormat))
                {
                    Title = stringsResourceLoader.GetString(Failed)
                }.ShowAsync();

                IsLoadingVisible = false;
                IsLoginFormVisible = true;
            }
        }

        private void OpenFolder(object sender)
        {
            const string Folder = "folder";
            if (selectedGridItem != null && selectedGridItem.Type == Folder)
            {
                currentPath = selectedGridItem.Path;
                _ = GetItemsAsync(selectedGridItem.Path);
            }
        }

        private void GetParent(object sender)
        {
            currentPath = currentPath.Substring(0, currentPath.LastIndexOf('/'));
            _ = GetItemsAsync(currentPath).ConfigureAwait(true);
        }

        private async Task GetItemsAsync(string path)
        {
            IsFilesVisible = false;
            IsLoadingVisible = true;
            IsBackButtonAvailable = false;

            var ftpFiles = await GetFilesFromFtpAsync(path).ConfigureAwait(true);
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
                    Image = themeResourceLoader.GetString(Folder),
                    DisplayName = elementName,
                    Type = Folder,
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
                        Image = themeResourceLoader.GetString(value),
                        DisplayName = elementName,
                        Type = File,
                        Path = currentPath + "/" + elementName
                    });
                }
                else
                {
                    items.Add(new OnlineFileControlViewModel()
                    {
                        Image = themeResourceLoader.GetString(File),
                        DisplayName = elementName,
                        Type = File,
                        Path = currentPath + "/" + elementName
                    });
                }

            }

            StorageFiles = new Collection<OnlineFileControlViewModel>(items);
            _ = CheckFilesForDownloadingAsync();
            IsLoadingVisible = false;
            IsFilesVisible = true;
            if (currentPath != hostLink)
            {
                IsBackButtonAvailable = true;
            }
        }

        private async Task CheckFilesForDownloadingAsync()
        {
            await Task.Run(() =>
            {
                foreach (var file in storageFiles)
                {
                    if (file.Type == Folder)
                    {
                        continue;
                    }
                    if (downloadingFilesPath.Exists(p => p == file.Path))
                    {
                        file.IsDownloading = true;
                        file.DownloadStatus = stringsResourceLoader.GetString(DownloadingText);
                    }
                }
            }).ConfigureAwait(true);
        }

        private async Task<List<string>> GetFilesFromFtpAsync(string path)
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
            response.Close();
            return files;
        }

        private async void DownloadFileAsync(object sender)
        {
            if (selectedGridItem != null && !string.IsNullOrEmpty(selectedGridItem.DisplayName) && selectedGridItem.Type != Folder)
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
                    SelectedGridItem.DownloadStatus = stringsResourceLoader.GetString(DownloadingText);
                    downloadingFilesPath.Add(selectedGridItem.Path);

                    _ = DownloadFtpFileAsync(destinationFile, selectedGridItem.Path);
                }
            }
        }

        private async Task DownloadFtpFileAsync(StorageFile destinationFile, string filePath)
        {
            bool isDownloadSuccess;
            Stream stream;

            try
            {
                using (WebClient client = new WebClient())
                {
                    client.Credentials = new NetworkCredential(username, password);
                    stream = await client.OpenReadTaskAsync(filePath).ConfigureAwait(true);
                }

                using (var fileStream = await destinationFile.OpenStreamForWriteAsync().ConfigureAwait(true))
                {
                    await stream.CopyToAsync(fileStream).ConfigureAwait(true);
                }
                isDownloadSuccess = true;
            }
            catch (WebException)
            {
                await new MessageDialog(stringsResourceLoader.GetString(ConnectionErrorContent))
                {
                    Title = stringsResourceLoader.GetString(ConnectionError)
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
                    downloadingFile.DownloadStatus = stringsResourceLoader.GetString(DownloadCompleted);
                }
                else
                {
                    downloadingFile.DownloadStatus = stringsResourceLoader.GetString(Failed);
                }
                _ = CloseDownloadingAsync(downloadingFile);
            }
        }

        private async Task CloseDownloadingAsync(OnlineFileControlViewModel file)
        {
            await Task.Delay(3000).ConfigureAwait(true);
            file.IsDownloading = false;
            file.DownloadStatus = string.Empty;
        }

        private async void UploadFileAsync(object sender)
        {
            var picker = new FileOpenPicker
            {
                ViewMode = PickerViewMode.Thumbnail,
                SuggestedStartLocation = PickerLocationId.Downloads
            };
            picker.FileTypeFilter.Add("*");
            StorageFile uploadFile = await picker.PickSingleFileAsync();

            if (uploadFile != null)
            {
                if (storageFiles.FirstOrDefault(f => f.DisplayName == uploadFile.Name) == null)
                {
                    _ = UploadFtpFileAsync(uploadFile, currentPath);
                }
                else
                {
                    var messageDialog = new MessageDialog(stringsResourceLoader.GetString(SameNameError))
                    {
                        Title = stringsResourceLoader.GetString(Failed)
                    };
                    await messageDialog.ShowAsync();
                }
            }
        }

        private async Task UploadFtpFileAsync(StorageFile uploadFile, string destinationPath)
        {
            bool isUploadSuccess;
            Stream responseStream;

            try
            {
                using (WebClient client = new WebClient())
                {
                    client.Credentials = new NetworkCredential(username, password);
                    responseStream = await client.OpenWriteTaskAsync(new Uri(destinationPath + "/" + uploadFile.Name)).ConfigureAwait(true);
                }

                using (var fileStream = await uploadFile.OpenStreamForWriteAsync().ConfigureAwait(true))
                {
                    await fileStream.CopyToAsync(responseStream).ConfigureAwait(true);
                }
                isUploadSuccess = true;
            }
            catch (WebException)
            {
                await new MessageDialog(stringsResourceLoader.GetString(ConnectionErrorContent))
                {
                    Title = stringsResourceLoader.GetString(ConnectionError)
                }.ShowAsync();
                isUploadSuccess = false;
            }

            if (isUploadSuccess && destinationPath == currentPath)
            {
                _ = GetItemsAsync(currentPath).ConfigureAwait(true);
            }
        }

        private async void DeleteFileAsync(object sender)
        {
            if (selectedGridItem != null && !string.IsNullOrEmpty(selectedGridItem.DisplayName))
            {
                var contentDialog = new ContentDialog()
                {
                    Title = stringsResourceLoader.GetString(Confirmation),
                    Content = stringsResourceLoader.GetString(DeleteConfirmText) + $" \"{selectedGridItem.DisplayName}\"?",
                    PrimaryButtonText = stringsResourceLoader.GetString(YesButton),
                    CloseButtonText = stringsResourceLoader.GetString(CancelButton),
                };
                var confirmationResult = await contentDialog.ShowAsync();

                if (confirmationResult == ContentDialogResult.Primary)
                {
                    try
                    {
                        FtpWebRequest request = (FtpWebRequest)WebRequest.Create(selectedGridItem.Path);
                        request.Credentials = new NetworkCredential(username, password);

                        if (selectedGridItem.Type == Folder)
                        {
                            request.Method = WebRequestMethods.Ftp.RemoveDirectory;
                        }
                        else
                        {
                            request.Method = WebRequestMethods.Ftp.DeleteFile;
                        }

                        FtpWebResponse response = (FtpWebResponse)await request.GetResponseAsync().ConfigureAwait(true);
                        response.Close();

                        _ = GetItemsAsync(currentPath).ConfigureAwait(true);
                    }
                    catch (WebException)
                    {
                        await new MessageDialog(stringsResourceLoader.GetString(ConnectionErrorContent))
                        {
                            Title = stringsResourceLoader.GetString(ConnectionError)
                        }.ShowAsync();
                    }
                }
            }
        }

        private async void CreateNewFolderAsync(object sender)
        {
            var parameters = new string[] { stringsResourceLoader.GetString(NewFolder), stringsResourceLoader.GetString(PlaceHolderFileName), string.Empty };

            var contentDialog = new ContentDialogControl()
            {
                PrimaryButtonText = stringsResourceLoader.GetString(CreateButton),
                SecondaryButtonText = stringsResourceLoader.GetString(CancelButton),
                DataContext = Activator.CreateInstance(typeof(ContentDialogControlViewModel), parameters)
            };
            var result = await contentDialog.ShowAsync();
            var gridItem = (ContentDialogControlViewModel)contentDialog.DataContext;
            var folderName = gridItem.InputText;

            if (result == ContentDialogResult.Primary && ItemNameValidation.Validate(folderName))
            {
                while (folderName.EndsWith(' '))
                {
                    folderName = folderName.Remove(folderName.Length - 1);
                }
                while (folderName.StartsWith(' '))
                {
                    folderName = folderName.Remove(0, 1);
                }

                if (storageFiles.FirstOrDefault(f => f.DisplayName == folderName) == null)
                {
                    try
                    {
                        FtpWebRequest request = (FtpWebRequest)WebRequest.Create(currentPath + "/" + folderName);
                        request.Credentials = new NetworkCredential(username, password);
                        request.Method = WebRequestMethods.Ftp.MakeDirectory;
                        FtpWebResponse response = (FtpWebResponse)await request.GetResponseAsync().ConfigureAwait(true);
                        response.Close();

                        _ = GetItemsAsync(currentPath).ConfigureAwait(true);
                    }
                    catch (WebException)
                    {
                        await new MessageDialog(stringsResourceLoader.GetString(ConnectionErrorContent))
                        {
                            Title = stringsResourceLoader.GetString(ConnectionError)
                        }.ShowAsync();
                    }
                }
                else
                {
                    var messageDialog = new MessageDialog(stringsResourceLoader.GetString(SameNameError))
                    {
                        Title = stringsResourceLoader.GetString(Failed)
                    };
                    await messageDialog.ShowAsync();
                }

            }
            else if (result == ContentDialogResult.Primary)
            {
                var messageDialog = new MessageDialog(stringsResourceLoader.GetString(InvalidInput))
                {
                    Title = stringsResourceLoader.GetString(InputError)
                };
                await messageDialog.ShowAsync();
            }
        }

        private async void RenameFileAsync(object sender)
        {
            if (selectedGridItem != null)
            {
                var parameters = new string[] { stringsResourceLoader.GetString(Rename), stringsResourceLoader.GetString(PlaceHolderFileName), selectedGridItem.DisplayName };
                var contentDialog = new ContentDialogControl()
                {
                    PrimaryButtonText = stringsResourceLoader.GetString(YesButton),
                    SecondaryButtonText = stringsResourceLoader.GetString(CancelButton),
                    DataContext = Activator.CreateInstance(typeof(ContentDialogControlViewModel), parameters)
                };
                var result = await contentDialog.ShowAsync();
                var gridItem = (ContentDialogControlViewModel)contentDialog.DataContext;
                var newFileName = gridItem.InputText;

                if (result == ContentDialogResult.Primary && ItemNameValidation.Validate(newFileName))
                {
                    while (newFileName.EndsWith(' '))
                    {
                        newFileName = newFileName.Remove(newFileName.Length - 1);
                    }
                    while (newFileName.StartsWith(' '))
                    {
                        newFileName = newFileName.Remove(0, 1);
                    }

                    if (storageFiles.FirstOrDefault(f => f.DisplayName == newFileName) == null)
                    {
                        try
                        {
                            FtpWebRequest request = (FtpWebRequest)WebRequest.Create(currentPath + "/" + selectedGridItem.DisplayName);
                            request.Credentials = new NetworkCredential(username, password);
                            request.Method = WebRequestMethods.Ftp.Rename;
                            request.RenameTo = newFileName;
                            FtpWebResponse response = (FtpWebResponse)await request.GetResponseAsync().ConfigureAwait(true);
                            response.Close();

                            _ = GetItemsAsync(currentPath).ConfigureAwait(true);
                        }
                        catch (WebException)
                        {
                            await new MessageDialog(stringsResourceLoader.GetString(ConnectionErrorContent))
                            {
                                Title = stringsResourceLoader.GetString(ConnectionError)
                            }.ShowAsync();
                        }
                    }
                    else
                    {
                        var messageDialog = new MessageDialog(stringsResourceLoader.GetString(SameNameError))
                        {
                            Title = stringsResourceLoader.GetString(Failed)
                        };
                        await messageDialog.ShowAsync();
                    }
                }
                else if (result == ContentDialogResult.Primary)
                {
                    var messageDialog = new MessageDialog(stringsResourceLoader.GetString(InvalidInput))
                    {
                        Title = stringsResourceLoader.GetString(InputError)
                    };
                    await messageDialog.ShowAsync();
                }
            }
        }
    }
}
