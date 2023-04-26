using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using ThirdPartyServices.Shared.Models;
using ThirdPartyServices.Shared.Models.Enums;
using ThirdPartyServices.Shared.Models.Responses.Microsoft;
using Windows.ApplicationModel.Core;
using Windows.ApplicationModel.Resources;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Popups;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml.Controls;
using Autofac;
using FileManager.Dependencies;
using FileManager.Helpers;
using FileManager.Helpers.Commands;
using FileManager.Helpers.Factory;
using FileManager.Helpers.Validation;
using FileManager.Models;
using FileManager.Models.Interfaces;
using FileManager.Services;
using FileManager.ViewModels.OnlineFileControls;

namespace FileManager.ViewModels
{
    public class OneDriveViewModel : BindableBase
    {
        private const string OneDriveUri = "https://graph.microsoft.com/v1.0/me/drive/items/";
        private readonly MicrosoftAuthParams microsoftParams = new MicrosoftAuthParams()
        {
            ClientId = "128ced40-e64a-454d-acb4-3752c08a0814", //"4d6596ac-0602-49e6-be9e-ed97fdea89f5",
            ClientSecret = "k9k8Q~BzgRKh59sc.C8lZtCyGexcf~NNGdxBudtN", //"Ofq8Q~ARZDOVNJ12brW9nUkIFEFeJ6eHxnA1-caD",
            RedirectUri = "http://localhost:3000",
            ScopeType = MicrosoftScope.OneDrive
        };
        private readonly TokenResult tokenResult;
        private readonly ResourceLoader stringsResourceLoader;
        private readonly Stack<string> openedFoldersId = new Stack<string>();
        private readonly List<string> downloadingFilesId;
        private readonly OneDriveService oneDriveService;
        private string loadingText;
        private string errorText;
        private string currentFolderId;
        private Uri webViewCurrentSource;
        private bool isCommandPanelVisible;
        private bool isLoadingVisible;
        private bool isFilesVisible;
        private bool isBackButtonAvailable;
        private bool isErrorVisible;
        private Collection<OnlineFileControlViewModel> storageFiles;
        private OnlineFileControlViewModel selectedGridItem;
        private ResourceLoader themeResourceLoader;
        private ICommand doubleClickedCommand;
        private ICommand getParentCommand;
        private ICommand downloadFileCommand;
        private ICommand uploadFileCommand;
        private ICommand deleteFileCommand;
        private ICommand createNewFolderCommand;
        private ICommand renameFileCommand;
        private ICommand itemClickedCommand;

        public Uri WebViewCurrentSource
        {
            get => webViewCurrentSource;
            set
            {
                if (webViewCurrentSource != value)
                {
                    webViewCurrentSource = value;
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
        public bool IsErrorVisible
        {
            get => isErrorVisible;
            set
            {
                if (isErrorVisible != value)
                {
                    isErrorVisible = value;
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
        public string ErrorText
        {
            get => errorText;
            set
            {
                if (errorText != value)
                {
                    errorText = value;
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
            get => createNewFolderCommand;
            set
            {
                if (createNewFolderCommand != value)
                {
                    createNewFolderCommand = value;
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

        public OneDriveViewModel()
        {
            tokenResult = new TokenResult();
            ChangeColorMode(settings, this);
            downloadingFilesId = new List<string>();
            oneDriveService = new OneDriveService();
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
            DoubleClickedCommand = new RelayCommand(OpenFolder);
            GetParentCommand = new RelayCommand(GetParent);
            DownloadFileCommand = new RelayCommand(DownloadFileAsync);
            UploadFileCommand = new RelayCommand(UploadFileAsync);
            DeleteFileCommand = new RelayCommand(DeleteFileAsync);
            CreateNewFolderCommand = new RelayCommand(CreateNewFolderAsync);
            RenameFileCommand = new RelayCommand(RenameFileAsync);
            stringsResourceLoader = ResourceLoader.GetForCurrentView(Constants.StringResources);
            _ = CheckInternetConnectionAsync();
            LoadingText = stringsResourceLoader.GetString(Constants.Loading);
            ErrorText = stringsResourceLoader.GetString(Constants.ConnectionErrorContent);
            _ = OneDriveAuthAsync();
        }

        protected override void ChangeColorMode(UISettings uiSettings, object sender)
        {
            var currentBackgroundColor = uiSettings?.GetColorValue(UIColorType.Background);
            string resourcePath;
            if (backgroundColor != currentBackgroundColor || storageFiles == null)
            {
                if (currentBackgroundColor == Colors.Black)
                {
                    resourcePath = string.Join('\\', Constants.Resources, Constants.ImagesDark);
                    backgroundColor = Colors.Black;
                }
                else
                {
                    resourcePath = string.Join('\\', Constants.Resources, Constants.ImagesLight);
                    backgroundColor = Colors.White;
                }
                themeResourceLoader = ResourceLoader.GetForViewIndependentUse(resourcePath);
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

        private async Task CheckInternetConnectionAsync()
        {
            Enums result = await oneDriveService.CheckInternetConnectionAsync(OneDriveUri).ConfigureAwait(true);
            if (result == Enums.Failed)
            {
                IsErrorVisible = true;
                IsCommandPanelVisible = false;
            }
        }

        private async Task OneDriveAuthAsync()
        {
            string errorContent;
            string errorTitle;
            var dialog = VMDependencies.Container.Resolve<IAuthWebViewDialog>();
            var dialogResult = dialog.ShowAsync(null);
            Enums result = await oneDriveService.OneDriveAuthAsync(microsoftParams, tokenResult);
            if (result == Enums.Failed)
            {
                errorContent = stringsResourceLoader.GetString(Constants.ConnectionErrorContent);
                errorTitle = stringsResourceLoader.GetString(Constants.ConnectionError);
                _ = ShowMessageDialogAsync(errorContent, errorTitle);
                IsErrorVisible = true;
            }
            else if (IsErrorVisible == false)
            {
                IsLoadingVisible = true;
                currentFolderId = "root";
                _ = GetItemsAsync(currentFolderId);
                IsCommandPanelVisible = true;
            }
            dialog.Dismiss();
        }
        private async Task GetItemsAsync(string folderId = "")
        {
            IsFilesVisible = false;
            IsLoadingVisible = true;
            _ = CheckInternetConnectionAsync();
            ItemsResponse responseFiles = await GetFilesFromOneDriveAsync(folderId).ConfigureAwait(true);
            List<OnlineFileControlViewModel> driveFiles = new List<OnlineFileControlViewModel>();
            foreach (var driveFile in responseFiles.Value)
            {
                string type;
                if (driveFile.File == null)
                {
                    type = Constants.Folder;
                }
                else
                {
                    type = driveFile.File.ToString().Split(':', ',')[1].Trim().Replace("\"", "");
                }
                var viewModel = OnlineFileControlCreator.CreateFileControl(themeResourceLoader, driveFile.Id, driveFile.Name, type);
                if (viewModel.Type == Constants.Folder)
                {
                    var lastIndexOfFolder = driveFiles.FindLastIndex(f => f.Type == Constants.Folder);
                    driveFiles.Insert(lastIndexOfFolder + 1, viewModel);
                }
                else
                {
                    driveFiles.Add(viewModel);
                }
            }
            StorageFiles = new Collection<OnlineFileControlViewModel>(driveFiles);
            CheckFilesForDownloading();
            IsLoadingVisible = false;
            IsFilesVisible = true;
        }

        private async Task<ItemsResponse> GetFilesFromOneDriveAsync(string folderId)
        {
            ItemsResponse driveFiles;
            string errorContent;
            string errorTitle;
            if (DateTime.Now.Subtract(tokenResult.LastRefreshTime).Seconds >= int.Parse(tokenResult.Expires_in))
            {
                await RefreshTokenAsync().ConfigureAwait(true);
            }
            driveFiles = await oneDriveService.GetFilesAsync(folderId, tokenResult.Access_token).ConfigureAwait(true);
            if (driveFiles.Value == null)
            {
                errorContent = stringsResourceLoader.GetString(Constants.ConnectionErrorContent);
                errorTitle = stringsResourceLoader.GetString(Constants.ConnectionError);
                _ = ShowMessageDialogAsync(errorContent, errorTitle);
                driveFiles.Value = new List<ItemResponse>();
            }
            return driveFiles;
        }
        private void CheckFilesForDownloading()
        {
            foreach (var file in storageFiles)
            {
                if (downloadingFilesId.Exists(id => id == file.Id))
                {
                    file.IsDownloading = true;
                    file.DownloadStatus = stringsResourceLoader.GetString(Constants.DownloadingText);
                }
            }
        }

        private async Task RefreshTokenAsync()
        {
            Enums result;
            string errorContent;
            string errorTitle;
            result = await oneDriveService.RefreshTokenAsync(microsoftParams, tokenResult).ConfigureAwait(true);
            if (result == Enums.Failed)
            {
                errorContent = stringsResourceLoader.GetString(Constants.ConnectionErrorContent);
                errorTitle = stringsResourceLoader.GetString(Constants.ConnectionError);
                _ = ShowMessageDialogAsync(errorContent, errorTitle);
            }
        }

        private void OpenFolder(object sender)
        {
            if (sender != null)
            {
                var gridItems = (GridView)sender;
                if (gridItems.SelectedItem is OnlineFileControlViewModel selectedItem && !string.IsNullOrEmpty(selectedItem.DisplayName) && selectedItem.Type == Constants.Folder)
                {
                    if (openedFoldersId.Count == 0)
                    {
                        IsBackButtonAvailable = true;
                    }
                    openedFoldersId.Push(currentFolderId);
                    currentFolderId = selectedItem.Id;
                    GetItemsAsync(currentFolderId).ConfigureAwait(true);
                }
            }

        }

        private void GetParent(object sender)
        {
            if (openedFoldersId.Count != 0)
            {
                currentFolderId = openedFoldersId.Pop();
                GetItemsAsync(currentFolderId).ConfigureAwait(true);
                if (openedFoldersId.Count == 0)
                {
                    IsBackButtonAvailable = false;
                }
            }
        }

        private async void DownloadFileAsync(object sender)
        {
            Enums result;
            string errorContent;
            string errorTitle;
            string fileId = selectedGridItem.Id;
            if (selectedGridItem != null && !string.IsNullOrEmpty(selectedGridItem.DisplayName) && selectedGridItem.Type != Constants.Folder)
            {
                StorageFolder downloadFolder = await GetDestinationFolderAsync().ConfigureAwait(true);
                if (DateTime.Now.Subtract(tokenResult.LastRefreshTime).Seconds >= int.Parse(tokenResult.Expires_in))
                {
                    await RefreshTokenAsync().ConfigureAwait(true);
                }
                SelectedGridItem.IsDownloading = true;
                SelectedGridItem.DownloadStatus = stringsResourceLoader.GetString(Constants.DownloadingText);
                downloadingFilesId.Add(SelectedGridItem.Id);
                result = await oneDriveService.DownloadFileAsync(downloadFolder, selectedGridItem.DisplayName,
                    selectedGridItem.Id, tokenResult.Access_token).ConfigureAwait(true);

                downloadingFilesId.Remove(fileId);
                var downloadingFile = storageFiles.FirstOrDefault(f => f.Id == fileId);
                if (downloadingFile != null)
                {
                    if (result == Enums.Success)
                    {
                        downloadingFile.DownloadStatus = stringsResourceLoader.GetString(Constants.DownloadCompleted);
                    }
                    else
                    {
                        downloadingFile.DownloadStatus = stringsResourceLoader.GetString(Constants.Failed);
                        errorContent = stringsResourceLoader.GetString(Constants.ConnectionErrorContent);
                        errorTitle = stringsResourceLoader.GetString(Constants.ConnectionError);
                        _ = ShowMessageDialogAsync(errorContent, errorTitle);
                    }
                    _ = CloseDownloadingAsync(downloadingFile);
                }
            }
        }
        private async void UploadFileAsync(object sender)
        {
            Enums result;
            string errorContent;
            string errorTitle;
            string folderId = currentFolderId;
            var picker = new FileOpenPicker
            {
                ViewMode = PickerViewMode.Thumbnail,
                SuggestedStartLocation = PickerLocationId.Downloads
            };
            picker.FileTypeFilter.Add("*");
            StorageFile uploadFile = await picker.PickSingleFileAsync();
            if (uploadFile != null)
            {
                if (DateTime.Now.Subtract(tokenResult.LastRefreshTime).Seconds >= int.Parse(tokenResult.Expires_in))
                {
                    await RefreshTokenAsync().ConfigureAwait(true);
                }
                result = await oneDriveService.UploadFileAsync(uploadFile, tokenResult.Access_token).ConfigureAwait(true);
                if (result == Enums.Success)
                {
                    if (folderId == currentFolderId)
                    {
                        _ = GetItemsAsync(currentFolderId);
                    }
                }
                else
                {
                    errorContent = stringsResourceLoader.GetString(Constants.ConnectionErrorContent);
                    errorTitle = stringsResourceLoader.GetString(Constants.ConnectionError);
                    _ = ShowMessageDialogAsync(errorContent, errorTitle);
                }
            }
        }

        private async void DeleteFileAsync(object sender)
        {
            string folderId = currentFolderId;
            Enums result;
            string errorContent;
            string errorTitle;
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
                    if (DateTime.Now.Subtract(tokenResult.LastRefreshTime).Seconds >= int.Parse(tokenResult.Expires_in))
                    {
                        await RefreshTokenAsync().ConfigureAwait(true);
                    }
                    result = await oneDriveService.DeleteFileAsync(selectedGridItem.Id, tokenResult.Access_token).ConfigureAwait(true);
                    if (result == Enums.Success)
                    {
                        if (currentFolderId == folderId)
                        {
                            _ = GetItemsAsync(currentFolderId).ConfigureAwait(true);
                        }
                    }
                    else
                    {
                        errorContent = stringsResourceLoader.GetString(Constants.ConnectionErrorContent);
                        errorTitle = stringsResourceLoader.GetString(Constants.ConnectionError);
                        _ = ShowMessageDialogAsync(errorContent, errorTitle);
                    }
                }
            }
        }

        private async void CreateNewFolderAsync(object sender)
        {
            string dialogTitle = stringsResourceLoader.GetString(Constants.NewFolder);
            string placeHolder = stringsResourceLoader.GetString(Constants.PlaceHolderFileName);
            string inputText = string.Empty;
            string primaryButton = stringsResourceLoader.GetString(Constants.CreateButton);
            string secondaryButton = stringsResourceLoader.GetString(Constants.CancelButton);
            Enums result;
            string errorContent;
            string errorTitle;
            var contentDialog = CreateInputContentDialog(dialogTitle, placeHolder, inputText, primaryButton, secondaryButton);
            var dialogResult = await contentDialog.ShowAsync();
            var gridItem = (ContentDialogControlViewModel)contentDialog.DataContext;
            var folderName = gridItem.InputText;
            folderName = ValidateItemName(folderName);
            if (dialogResult == ContentDialogResult.Primary && !string.IsNullOrEmpty(folderName))
            {
                if (DateTime.Now.Subtract(tokenResult.LastRefreshTime).Seconds >= int.Parse(tokenResult.Expires_in))
                {
                    await RefreshTokenAsync().ConfigureAwait(true);
                }
                result = await oneDriveService.CreateNewFolderAsync(folderName, currentFolderId, tokenResult.Token_type, tokenResult.Access_token).ConfigureAwait(true);
                if (result == Enums.Success)
                {
                    _ = GetItemsAsync(currentFolderId);
                }
                else
                {
                    errorContent = stringsResourceLoader.GetString(Constants.ConnectionErrorContent);
                    errorTitle = stringsResourceLoader.GetString(Constants.ConnectionError);
                    _ = ShowMessageDialogAsync(errorContent, errorTitle);
                }
            }
            else if (dialogResult == ContentDialogResult.Primary)
            {
                errorContent = stringsResourceLoader.GetString(Constants.InvalidInput);
                errorTitle = stringsResourceLoader.GetString(Constants.InputError);
                _ = ShowMessageDialogAsync(errorContent, errorTitle);
            }
        }

        private async void RenameFileAsync(object sender)
        {
            string dialogTitle = stringsResourceLoader.GetString(Constants.Rename);
            string placeHolder = stringsResourceLoader.GetString(Constants.PlaceHolderFileName);
            string primaryButton = stringsResourceLoader.GetString(Constants.YesButton);
            string secondaryButton = stringsResourceLoader.GetString(Constants.CancelButton);
            Enums result;
            if (selectedGridItem != null)
            {
                string inputText = selectedGridItem.DisplayName;
                string errorContent;
                string errorTitle;
                var contentDialog = CreateInputContentDialog(dialogTitle, placeHolder, inputText, primaryButton, secondaryButton);
                var dialogResult = await contentDialog.ShowAsync();
                var gridItem = (ContentDialogControlViewModel)contentDialog.DataContext;
                var fileName = gridItem.InputText;
                fileName = ValidateItemName(fileName);

                if (dialogResult == ContentDialogResult.Primary && !string.IsNullOrEmpty(fileName))
                {
                    if (DateTime.Now.Subtract(tokenResult.LastRefreshTime).Seconds >= int.Parse(tokenResult.Expires_in))
                    {
                        await RefreshTokenAsync().ConfigureAwait(true);
                    }
                    result = await oneDriveService.RenameFileAsync(selectedGridItem.Id, fileName, tokenResult.Token_type, tokenResult.Access_token).ConfigureAwait(true);
                    if (result == Enums.Success)
                    {
                        _ = GetItemsAsync(currentFolderId);
                    }
                    else
                    {
                        errorContent = stringsResourceLoader.GetString(Constants.ConnectionErrorContent);
                        errorTitle = stringsResourceLoader.GetString(Constants.ConnectionError);
                        _ = ShowMessageDialogAsync(errorContent, errorTitle);
                    }
                }
                else if (dialogResult == ContentDialogResult.Primary)
                {
                    errorContent = stringsResourceLoader.GetString(Constants.InvalidInput);
                    errorTitle = stringsResourceLoader.GetString(Constants.InputError);
                    _ = ShowMessageDialogAsync(errorContent, errorTitle);
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
        private async Task ShowMessageDialogAsync(string content, string title)
        {
            var messageDialog = new MessageDialog(content)
            {
                Title = title
            };
            await messageDialog.ShowAsync();
        }
        private ContentDialog CreateInputContentDialog(string title, string placeHolder, string inputText, string primaryButton, string secondaryButton)
        {
            var parameters = new List<NamedParameter>()
                {
                    new NamedParameter("title", title),
                    new NamedParameter("placeHolder", placeHolder),
                    new NamedParameter("primaryButtonText", primaryButton),
                    new NamedParameter("secondaryButtonText", secondaryButton),
                    new NamedParameter("inputText", inputText),
                };
            var contentDialog = (ContentDialog)VMDependencies.Container.Resolve(VMDependencies.Views["ContentDialogControl"]);
            contentDialog.DataContext = VMDependencies.Container.Resolve<ContentDialogControlViewModel>(parameters);
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
