using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
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
using FileManager.Commands;
using FileManager.Controlls;
using FileManager.Helpers;
using FileManager.Services;
using FileManager.Validation;
using FileManager.ViewModels.OnlineFileControls;
using FileManager.Factory;
using FileManager.Models;

namespace FileManager.ViewModels
{
    public class FtpViewModel : BindableBase
    {
        private const string ProtocolName = "ftp://";
        private readonly List<string> downloadingFilesPath;
        private readonly Dictionary<string, string> knownTypes;
        private readonly ResourceLoader stringsResourceLoader;
        private readonly FtpService ftpService;
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
        private OnlineFileControlViewModel selectedGridItem;
        private ResourceLoader themeResourceLoader;
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
                { ".jpg", Constants.Image },
                { ".jpeg", Constants.Image },
                { ".jfif", Constants.Image },
                { ".gif", Constants.Image },
                { ".png", Constants.Image },
                { ".mp4", Constants.Video },
                { ".avi", Constants.Video },
                { ".wmv", Constants.Video },
                { ".amv", Constants.Video },
                { ".mp3", Constants.Audio },
                { ".ogg", Constants.Audio },
                { ".wma", Constants.Audio },
                { ".wav", Constants.Audio },
            };
            downloadingFilesPath = new List<string>();
            ftpService = new FtpService();
            stringsResourceLoader = ResourceLoader.GetForCurrentView(Constants.Resources);
            LoadingText = stringsResourceLoader.GetString(Constants.Loading);
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
                    themeResourceLoader = ResourceLoader.GetForViewIndependentUse(Constants.ImagesDark);
                    backgroundColor = Colors.Black;
                }
                else
                {
                    themeResourceLoader = ResourceLoader.GetForViewIndependentUse(Constants.ImagesLight);
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
                            storageFile.ChangeColorMode(themeResourceLoader);
                        }
                    }).AsTask().ConfigureAwait(true);
                }
            }
        }

        private async void ConnectAsync(object sender)
        {
            IsLoginFormVisible = false;
            IsLoadingVisible = true;
            var result = await ftpService.TryConnectAsync(hostLink, username, password).ConfigureAwait(true);
            if (result == Constants.Success)
            {
                currentPath = HostLink;
                IsCommandPanelVisible = true;
                _ = GetItemsAsync(currentPath).ConfigureAwait(true);
            }
            else if (result == Constants.InvalidUriFormat)
            {
                await ShowMessageDialogAsync(stringsResourceLoader.GetString(Constants.InvalidUriFormat),
                            stringsResourceLoader.GetString(Constants.Failed)).ConfigureAwait(true);
                IsLoadingVisible = false;
                IsLoginFormVisible = true;
            }
            else
            {
                await ShowMessageDialogAsync(stringsResourceLoader.GetString(Constants.ConnectionErrorContent),
                    stringsResourceLoader.GetString(Constants.ConnectionError)).ConfigureAwait(true);
                IsLoadingVisible = false;
                IsLoginFormVisible = true;
            }
        }

        private void OpenFolder(object sender)
        {
            if (selectedGridItem != null && selectedGridItem.Type == Constants.Folder)
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

            var ftpFiles = await ftpService.GetFilesAsync(path, username, password).ConfigureAwait(true);
            List<OnlineFileControlViewModel> items = new List<OnlineFileControlViewModel>();
            foreach (var file in ftpFiles)
            {
                var filePath = string.Join("/", path, file.Name);
                var viewModel = OnlineFileControlCreator.CreateFileControl(themeResourceLoader, file.Id, file.Name, file.Type, filePath);
                if (viewModel.Type == Constants.Folder)
                {
                    var lastIndexOfFolder = items.FindLastIndex(f => f.Type == Constants.Folder);
                    items.Insert(lastIndexOfFolder + 1, viewModel);
                }
                else
                {
                    items.Add(viewModel);
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
                    if (file.Type == Constants.Folder)
                    {
                        continue;
                    }
                    if (downloadingFilesPath.Exists(p => p == file.Path))
                    {
                        file.IsDownloading = true;
                        file.DownloadStatus = stringsResourceLoader.GetString(Constants.DownloadingText);
                    }
                }
            }).ConfigureAwait(true);
        }

        private async void DownloadFileAsync(object sender)
        {
            string filePath = selectedGridItem.Path;
            string result;
            if (selectedGridItem != null && !string.IsNullOrEmpty(selectedGridItem.DisplayName) && selectedGridItem.Type != Constants.Folder)
            {
                StorageFolder downloadFolder = await GetDestinationFolderAsync().ConfigureAwait(true);

                if (downloadFolder != null)
                {
                    SelectedGridItem.IsDownloading = true;
                    SelectedGridItem.DownloadStatus = stringsResourceLoader.GetString(Constants.DownloadingText);
                    downloadingFilesPath.Add(selectedGridItem.Path);
                    result = await ftpService.DownloadFileAsync(downloadFolder, filePath, username, password).ConfigureAwait(true);
                    downloadingFilesPath.Remove(filePath);
                    var downloadingFile = storageFiles.FirstOrDefault(f => f.Path == filePath);
                    if (downloadingFile != null)
                    {
                        if (result == Constants.Success)
                        {
                            downloadingFile.DownloadStatus = stringsResourceLoader.GetString(Constants.DownloadCompleted);
                        }
                        else
                        {
                            await ShowMessageDialogAsync(stringsResourceLoader.GetString(Constants.ConnectionErrorContent),
                                stringsResourceLoader.GetString(Constants.ConnectionError)).ConfigureAwait(true);
                            downloadingFile.DownloadStatus = stringsResourceLoader.GetString(Constants.Failed);
                        }
                        _ = CloseDownloadingAsync(downloadingFile);
                    }
                }
            }
        }
        private async Task<StorageFolder> GetDestinationFolderAsync()
        {
            var picker = new FolderPicker
            {
                ViewMode = PickerViewMode.Thumbnail,
                SuggestedStartLocation = PickerLocationId.Downloads
            };
            picker.FileTypeFilter.Add("*");
            return await picker.PickSingleFolderAsync();
        }
        private async Task CloseDownloadingAsync(OnlineFileControlViewModel file)
        {
            await Task.Delay(3000).ConfigureAwait(true);
            file.IsDownloading = false;
            file.DownloadStatus = string.Empty;
        }

        private async void UploadFileAsync(object sender)
        {
            string result;
            string destinationPath = currentPath;
            StorageFile uploadFile = await GetUploadFileAsync().ConfigureAwait(true);

            if (uploadFile != null)
            {
                if (storageFiles.FirstOrDefault(f => f.DisplayName == uploadFile.Name) == null)
                {
                    result = await ftpService.UploadFileAsync(uploadFile, currentPath, username, password).ConfigureAwait(true);
                    if (result == Constants.Success && destinationPath == currentPath)
                    {
                        _ = GetItemsAsync(currentPath).ConfigureAwait(true);
                    }
                    else
                    {
                        await ShowMessageDialogAsync(stringsResourceLoader.GetString(Constants.ConnectionErrorContent),
                            stringsResourceLoader.GetString(Constants.ConnectionError)).ConfigureAwait(true);
                    }
                }
                else
                {
                    await ShowMessageDialogAsync(stringsResourceLoader.GetString(Constants.SameNameError),
                        stringsResourceLoader.GetString(Constants.Failed)).ConfigureAwait(true);
                }
            }
        }
        private async Task<StorageFile> GetUploadFileAsync()
        {
            var picker = new FileOpenPicker
            {
                ViewMode = PickerViewMode.Thumbnail,
                SuggestedStartLocation = PickerLocationId.Downloads
            };
            picker.FileTypeFilter.Add("*");
            return await picker.PickSingleFileAsync();
        }

        private async void DeleteFileAsync(object sender)
        {
            string deletingResult;
            if (selectedGridItem != null && !string.IsNullOrEmpty(selectedGridItem.DisplayName))
            {
                var contentDialog = new ContentDialog()
                {
                    Title = stringsResourceLoader.GetString(Constants.Confirmation),
                    Content = stringsResourceLoader.GetString(Constants.DeleteConfirmText) + $" \"{selectedGridItem.DisplayName}\"?",
                    PrimaryButtonText = stringsResourceLoader.GetString(Constants.YesButton),
                    CloseButtonText = stringsResourceLoader.GetString(Constants.CancelButton),
                };
                var confirmationResult = await contentDialog.ShowAsync();

                if (confirmationResult == ContentDialogResult.Primary)
                {
                    deletingResult = await ftpService.DeleteFileAsync(selectedGridItem.Path, selectedGridItem.Type, username, password).ConfigureAwait(true);
                    if (deletingResult == Constants.Success)
                    {
                        _ = GetItemsAsync(currentPath).ConfigureAwait(true);
                    }
                    else
                    {
                        await ShowMessageDialogAsync(stringsResourceLoader.GetString(Constants.ConnectionErrorContent),
                            stringsResourceLoader.GetString(Constants.ConnectionError)).ConfigureAwait(true);
                    }
                }
            }
        }

        private async void CreateNewFolderAsync(object sender)
        {
            string creatingResult;
            string dialogTitle = stringsResourceLoader.GetString(Constants.NewFolder);
            string placeHolder = stringsResourceLoader.GetString(Constants.PlaceHolderFileName);
            string inputText = string.Empty;
            string primaryButton = stringsResourceLoader.GetString(Constants.CreateButton);
            string secondaryButton = stringsResourceLoader.GetString(Constants.CancelButton);
            var contentDialog = CreateInputContentDialog(dialogTitle, placeHolder, inputText, primaryButton, secondaryButton);
            var dialogResult = await contentDialog.ShowAsync();
            var gridItem = (ContentDialogControlViewModel)contentDialog.DataContext;
            var folderName = gridItem.InputText;
            folderName = ValidateItemName(folderName);
            if (dialogResult == ContentDialogResult.Primary && !string.IsNullOrEmpty(folderName))
            {
                if (storageFiles.FirstOrDefault(f => f.DisplayName == folderName) == null)
                {
                    creatingResult = await ftpService.CreateNewFolderAsync(currentPath, folderName, username, password).ConfigureAwait(true);
                    if (creatingResult == Constants.Success)
                    {
                        _ = GetItemsAsync(currentPath).ConfigureAwait(true);
                    }
                    else
                    {
                        await ShowMessageDialogAsync(stringsResourceLoader.GetString(Constants.ConnectionErrorContent),
                            stringsResourceLoader.GetString(Constants.ConnectionError)).ConfigureAwait(true);
                    }
                }
                else
                {
                    await ShowMessageDialogAsync(stringsResourceLoader.GetString(Constants.SameNameError),
                        stringsResourceLoader.GetString(Constants.Failed)).ConfigureAwait(true);
                }

            }
            else if (dialogResult == ContentDialogResult.Primary)
            {
                await ShowMessageDialogAsync(stringsResourceLoader.GetString(Constants.InvalidInput),
                        stringsResourceLoader.GetString(Constants.InputError)).ConfigureAwait(true);
            }
        }

        private async void RenameFileAsync(object sender)
        {
            string dialogTitle = stringsResourceLoader.GetString(Constants.Rename);
            string placeHolder = stringsResourceLoader.GetString(Constants.PlaceHolderFileName);
            string inputText = selectedGridItem.DisplayName;
            string primaryButton = stringsResourceLoader.GetString(Constants.YesButton);
            string secondaryButton = stringsResourceLoader.GetString(Constants.CancelButton);
            string result;
            if (selectedGridItem != null)
            {
                var contentDialog = CreateInputContentDialog(dialogTitle, placeHolder, inputText, primaryButton, secondaryButton);
                var dialogResult = await contentDialog.ShowAsync();
                var gridItem = (ContentDialogControlViewModel)contentDialog.DataContext;
                var newFileName = gridItem.InputText;
                newFileName = ValidateItemName(newFileName);
                if (dialogResult == ContentDialogResult.Primary && !string.IsNullOrEmpty(newFileName))
                {
                    if (storageFiles.FirstOrDefault(f => f.DisplayName == newFileName) == null)
                    {
                        result = await ftpService.RenameFileAsync(currentPath, selectedGridItem.DisplayName, newFileName, username, password).ConfigureAwait(true);
                        if (result == Constants.Success)
                        {
                            _ = GetItemsAsync(currentPath).ConfigureAwait(true);
                        }
                        else
                        {
                            await ShowMessageDialogAsync(stringsResourceLoader.GetString(Constants.ConnectionErrorContent),
                                stringsResourceLoader.GetString(Constants.ConnectionError)).ConfigureAwait(true);
                        }
                    }
                    else
                    {
                        await ShowMessageDialogAsync(stringsResourceLoader.GetString(Constants.SameNameError),
                            stringsResourceLoader.GetString(Constants.Failed)).ConfigureAwait(true);
                    }
                }
                else if (dialogResult == ContentDialogResult.Primary)
                {
                    await ShowMessageDialogAsync(stringsResourceLoader.GetString(Constants.InvalidInput),
                            stringsResourceLoader.GetString(Constants.InputError)).ConfigureAwait(true);
                }
            }
        }
        private async Task ShowMessageDialogAsync(string content, string title)
        {
            var messageDialog = new MessageDialog(content)
            {
                Title = title
            };
            await messageDialog.ShowAsync();
        }
        private ContentDialogControl CreateInputContentDialog(string title, string placeHolder, string inputText, string primaryButton, string secondaryButton)
        {
            var parameters = new string[]
                {
                    title,
                    placeHolder,
                    inputText
                };
            var contentDialog = new ContentDialogControl()
            {
                PrimaryButtonText = primaryButton,
                SecondaryButtonText = secondaryButton,
                DataContext = Activator.CreateInstance(typeof(ContentDialogControlViewModel), parameters)
            };
            return contentDialog;
        }
        private string ValidateItemName(string itemName)
        {
            string result = string.Empty;
            if (ItemNameValidation.Validate(itemName))
            {
                while (itemName.EndsWith(' '))
                {
                    itemName = itemName.Remove(itemName.Length - 1);
                }
                while (itemName.StartsWith(' '))
                {
                    itemName = itemName.Remove(0, 1);
                }
                result = itemName;
            }
            return result;
        }

    }
}
