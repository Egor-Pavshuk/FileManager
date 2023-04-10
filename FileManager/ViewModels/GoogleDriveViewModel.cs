using FileManager.Commands;
using FileManager.Controlls;
using FileManager.Helpers;
using FileManager.Models;
using FileManager.Services;
using FileManager.Validation;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.ApplicationModel.Core;
using Windows.ApplicationModel.Resources;
using Windows.Data.Json;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Popups;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml.Controls;

namespace FileManager.ViewModels
{
    public class GoogleDriveViewModel : BindableBase
    {
        private const string ClientId = "148004585806-56makt200p390b18n2q21ugq71tb2msg.apps.googleusercontent.com";
        private const string Secret = "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";
        private const string GoogleUri = "https://accounts.google.com/o/oauth2/auth?client_id=" + ClientId + "&redirect_uri=" + "https://localhost/" + "&response_type=code&scope=" + Constants.DriveScope;
        private const string GoogleDownloadUri = "https://www.googleapis.com/drive/v3/files/";
        private readonly TokenResult tokenResult;
        private readonly ResourceLoader stringsResourceLoader;
        private readonly Stack<string> openedFoldersId = new Stack<string>();
        private readonly List<string> downloadingFilesId;
        private readonly GoogleDriveService googleDriveService;
        private string loadingText;
        private string errorText;
        private string currentFolderId;
        private Uri webViewCurrentSource;
        private bool isWebViewVisible;
        private bool isCommandPanelVisible;
        private bool isLoadingVisible;
        private bool isFilesVisible;
        private bool isBackButtonAvailable;
        private bool isErrorVisible;
        private Collection<OnlineFileControlViewModel> storageFiles;
        private OnlineFileControlViewModel selectedGridItem;
        private ResourceLoader themeResourceLoader;
        private ICommand navigationStartingCommand;
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
        public bool IsWebViewVisible
        {
            get => isWebViewVisible;
            set
            {
                if (isWebViewVisible != value)
                {
                    isWebViewVisible = value;
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
        public ICommand NavigationStartingCommand
        {
            get => navigationStartingCommand;
            set
            {
                if (navigationStartingCommand != value)
                {
                    navigationStartingCommand = value;
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

        public GoogleDriveViewModel()
        {
            tokenResult = new TokenResult();
            ChangeColorMode(settings, this);
            downloadingFilesId = new List<string>();
            googleDriveService = new GoogleDriveService();
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
            NavigationStartingCommand = new RelayCommand(NavigationStarting);
            DoubleClickedCommand = new RelayCommand(OpenFolder);
            GetParentCommand = new RelayCommand(GetParent);
            DownloadFileCommand = new RelayCommand(DownloadFileAsync);
            UploadFileCommand = new RelayCommand(UploadFileAsync);
            DeleteFileCommand = new RelayCommand(DeleteFileAsync);
            CreateNewFolderCommand = new RelayCommand(CreateNewFolderAsync);
            RenameFileCommand = new RelayCommand(RenameFileAsync);
            stringsResourceLoader = ResourceLoader.GetForCurrentView(Constants.Resources);
            IsWebViewVisible = true;
            _ = CheckInternetConnectionAsync();
            WebViewCurrentSource = new Uri(GoogleUri);
            LoadingText = stringsResourceLoader.GetString(Constants.Loading);
            ErrorText = stringsResourceLoader.GetString(Constants.ConnectionErrorContent);
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
                            switch (storageFile.Type)
                            {
                                case Constants.Image:
                                    storageFile.Image = themeResourceLoader.GetString(Constants.Image);
                                    break;
                                case Constants.Video:
                                    storageFile.Image = themeResourceLoader.GetString(Constants.Video);
                                    break;
                                case Constants.Audio:
                                    storageFile.Image = themeResourceLoader.GetString(Constants.Audio);
                                    break;
                                case Constants.Folder:
                                    storageFile.Image = themeResourceLoader.GetString(Constants.Folder);
                                    break;
                                default:
                                    storageFile.Image = themeResourceLoader.GetString(Constants.File);
                                    break;
                            }
                        }
                    }).AsTask().ConfigureAwait(true);
                }
            }
        }

        private async Task CheckInternetConnectionAsync()
        {
            string result = await googleDriveService.CheckInternetConnectionAsync(GoogleUri).ConfigureAwait(true);
            if (result == Constants.Failed)
            {
                IsErrorVisible = true;
                IsCommandPanelVisible = false;
                IsWebViewVisible = false;
            }
        }

        private async void NavigationStarting(object args)
        {
            var webView = (WebViewNavigationStartingEventArgs)args;
            if (webView != null)
            {
                if (webView.Uri.ToString().StartsWith("https://localhost/", StringComparison.Ordinal))
                {
                    string navigationUri = webView.Uri.ToString();
                    if (navigationUri.Contains("code=", StringComparison.Ordinal))
                    {
                        string exchangeCode = navigationUri.Substring(navigationUri.IndexOf('=') + 1, navigationUri.IndexOf('&') - navigationUri.IndexOf('=') - 1);
                        webView.Cancel = true;
                        IsWebViewVisible = false;
                        IsCommandPanelVisible = true;
                        await ExchangeCodeOnTokenAsync(exchangeCode).ConfigureAwait(true);
                        _ = GetItemsAsync().ConfigureAwait(true);
                    }
                    else
                    {
                        await new MessageDialog(stringsResourceLoader.GetString(Constants.ResponseError) + ".")
                        {
                            Title = stringsResourceLoader.GetString(Constants.Failed) + "!"
                        }.ShowAsync();
                    }
                }
                else if (webView.Uri.ToString().StartsWith("https://support.google.com", StringComparison.Ordinal))
                {
                    webView.Cancel = true;
                    IsWebViewVisible = false;
                    IsErrorVisible = true;
                }
            }
        }

        private async Task ExchangeCodeOnTokenAsync(string exchangeCode)
        {
            string errorContent;
            string errorTitle;
            string result = await googleDriveService.ExchangeCodeOnTokenAsync(exchangeCode, ClientId, Secret, tokenResult).ConfigureAwait(true);
            if (result == Constants.Failed)
            {
                errorContent = stringsResourceLoader.GetString(Constants.ConnectionErrorContent);
                errorTitle = stringsResourceLoader.GetString(Constants.ConnectionError);
                _ = ShowMessageDialogAsync(errorContent, errorTitle);
            }
        }

        private async Task GetItemsAsync(string folderId = "")
        {
            IsFilesVisible = false;
            IsLoadingVisible = true;
            _ = CheckInternetConnectionAsync();
            JsonArray responseFiles = await GetFilesFromGoogleDriveAsync(folderId).ConfigureAwait(true);
            List<OnlineFileControlViewModel> driveFiles = new List<OnlineFileControlViewModel>();

            foreach (var driveFile in responseFiles)
            {
                var currentFile = JsonObject.Parse(driveFile.Stringify());
                var type = currentFile[Constants.MimeType].ToString();
                if (!type.Contains("." + Constants.Folder, StringComparison.Ordinal))
                {
                    continue;
                }

                OnlineFileControlViewModel viewModel;
                string currentFileName = currentFile["name"].ToString();
                viewModel = new OnlineFileControlViewModel()
                {
                    Id = currentFile["id"].ToString(),
                    Image = themeResourceLoader.GetString(Constants.Folder),
                    DisplayName = currentFileName.Substring(1, currentFileName.Length - 2),
                    Type = Constants.Folder
                };
                driveFiles.Add(viewModel);
            }

            foreach (var driveFile in responseFiles)
            {
                OnlineFileControlViewModel viewModel;
                var currentFile = JsonObject.Parse(driveFile.Stringify());
                var type = currentFile[Constants.MimeType].ToString();
                if (type.Contains("." + Constants.Folder, StringComparison.Ordinal))
                {
                    continue;
                }
                string currentFileName = currentFile["name"].ToString();

                if (type.Contains("." + Constants.Photo, StringComparison.Ordinal) || type.Contains("." + Constants.Shortcut, StringComparison.Ordinal) || type.Contains(Constants.Photo, StringComparison.Ordinal) || type.Contains(Constants.Image, StringComparison.Ordinal))
                {
                    viewModel = new OnlineFileControlViewModel()
                    {
                        Id = currentFile["id"].ToString(),
                        Image = themeResourceLoader.GetString(Constants.Image),
                        DisplayName = currentFileName.Substring(1, currentFileName.Length - 2),
                        Type = Constants.Image
                    };
                }
                else if (type.Contains("." + Constants.Video, StringComparison.Ordinal) || type.Contains(Constants.Video, StringComparison.Ordinal))
                {
                    viewModel = new OnlineFileControlViewModel()
                    {
                        Id = currentFile["id"].ToString(),
                        Image = themeResourceLoader.GetString(Constants.Video),
                        DisplayName = currentFileName.Substring(1, currentFileName.Length - 2),
                        Type = Constants.Video
                    };
                }
                else if (type.Contains("." + Constants.Audio, StringComparison.Ordinal) || type.Contains(Constants.Audio, StringComparison.Ordinal))
                {
                    viewModel = new OnlineFileControlViewModel()
                    {
                        Id = currentFile["id"].ToString(),
                        Image = themeResourceLoader.GetString(Constants.Audio),
                        DisplayName = currentFileName.Substring(1, currentFileName.Length - 2),
                        Type = Constants.Audio
                    };
                }
                else
                {
                    viewModel = new OnlineFileControlViewModel()
                    {
                        Id = currentFile["id"].ToString(),
                        Image = themeResourceLoader.GetString(Constants.File),
                        DisplayName = currentFileName.Substring(1, currentFileName.Length - 2),
                        Type = Constants.File
                    };
                }
                driveFiles.Add(viewModel);
            }

            StorageFiles = new Collection<OnlineFileControlViewModel>(driveFiles);
            CheckFilesForDownloading();
            IsLoadingVisible = false;
            IsFilesVisible = true;
        }

        private async Task<JsonArray> GetFilesFromGoogleDriveAsync(string folderId)
        {
            JsonArray driveFiles;
            string q;
            string errorContent;
            string errorTitle;
            if (string.IsNullOrEmpty(folderId))
            {
                currentFolderId = await GetRootFolderIdAsync().ConfigureAwait(true);
                q = $"{currentFolderId}+in+parents and trashed=false&fields=files(id,+mimeType,+name)";
            }
            else
            {
                q = $"{folderId}+in+parents and trashed=false&fields=files(*)";
            }
            driveFiles = await googleDriveService.GetFilesAsync(q, tokenResult.TokenType, tokenResult.AccessToken).ConfigureAwait(true);
            if (driveFiles == null)
            {
                errorContent = stringsResourceLoader.GetString(Constants.ConnectionErrorContent);
                errorTitle = stringsResourceLoader.GetString(Constants.ConnectionError);
                _ = ShowMessageDialogAsync(errorContent, errorTitle);
                driveFiles = new JsonArray();
            }
            return driveFiles;
        }

        private async Task<string> GetRootFolderIdAsync()
        {
            string rootFolderId;
            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(tokenResult.TokenType, tokenResult.AccessToken);
                var rootFolderResult = await client.GetAsync($"https://www.googleapis.com/drive/v3/files/root").ConfigureAwait(true);
                var rootFolderString = await rootFolderResult.Content.ReadAsStringAsync().ConfigureAwait(true);
                rootFolderId = JsonObject.Parse(rootFolderString)["id"].ToString();
            }
            return rootFolderId;
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
            string result;
            string errorContent;
            string errorTitle;
            result = await googleDriveService.RefreshTokenAsync(ClientId, Secret, tokenResult).ConfigureAwait(true);
            if (result == Constants.Failed)
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
            string result;
            string errorContent;
            string errorTitle;
            string fileId = selectedGridItem.Id;            
            if (selectedGridItem != null && !string.IsNullOrEmpty(selectedGridItem.DisplayName) && selectedGridItem.Type != Constants.Folder)
            {
                var picker = new FolderPicker
                {
                    ViewMode = PickerViewMode.Thumbnail,
                    SuggestedStartLocation = PickerLocationId.Downloads
                };
                picker.FileTypeFilter.Add("*");
                StorageFolder downloadFolder = await picker.PickSingleFolderAsync();
                if (DateTime.Now.Subtract(tokenResult.LastRefreshTime).Seconds >= int.Parse(tokenResult.ExpiresIn))
                {
                    await RefreshTokenAsync().ConfigureAwait(true);
                }
                Uri source = new Uri(GoogleDownloadUri + selectedGridItem.Id.Substring(1, selectedGridItem.Id.Length - 2) + "?alt=media");
                SelectedGridItem.IsDownloading = true;
                SelectedGridItem.DownloadStatus = stringsResourceLoader.GetString(Constants.DownloadingText);
                downloadingFilesId.Add(SelectedGridItem.Id);
                result = await googleDriveService.DownloadFileAsync(source, downloadFolder, selectedGridItem.DisplayName, 
                    tokenResult.TokenType, tokenResult.AccessToken).ConfigureAwait(true);
                downloadingFilesId.Remove(fileId);
                var downloadingFile = storageFiles.FirstOrDefault(f => f.Id == fileId);
                if (downloadingFile != null)
                {
                    if (result == Constants.Success)
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

        private async Task CloseDownloadingAsync(OnlineFileControlViewModel file)
        {
            await Task.Delay(3000).ConfigureAwait(true);
            file.IsDownloading = false;
            file.DownloadStatus = string.Empty;
        }

        private async void UploadFileAsync(object sender)
        {
            string result;
            string errorContent;
            string errorTitle;
            string folderId = currentFolderId;
            var parents = new Collection<string>()
            {
                currentFolderId.Substring(1, currentFolderId.Length - 2)
            };
            var picker = new FileOpenPicker
            {
                ViewMode = PickerViewMode.Thumbnail,
                SuggestedStartLocation = PickerLocationId.Downloads
            };
            picker.FileTypeFilter.Add("*");
            StorageFile uploadFile = await picker.PickSingleFileAsync();
            if (uploadFile != null)
            {
                if (DateTime.Now.Subtract(tokenResult.LastRefreshTime).Seconds >= int.Parse(tokenResult.ExpiresIn))
                {
                    await RefreshTokenAsync().ConfigureAwait(true);
                }                
                result = await googleDriveService.UploadFileAsync(uploadFile, parents, tokenResult.AccessToken).ConfigureAwait(true);
                if (result == Constants.Success)
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
            string result;
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
                    if (DateTime.Now.Subtract(tokenResult.LastRefreshTime).Seconds >= int.Parse(tokenResult.ExpiresIn))
                    {
                        await RefreshTokenAsync().ConfigureAwait(true);
                    }
                    result = await googleDriveService.DeleteFileAsync(selectedGridItem.Id, tokenResult.AccessToken).ConfigureAwait(true);
                    if (result == Constants.Success)
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
            string dialogTitle = stringsResourceLoader.GetString(Constants.Rename);
            string placeHolder = stringsResourceLoader.GetString(Constants.PlaceHolderFileName);
            string inputText = selectedGridItem.DisplayName;
            string primaryButton = stringsResourceLoader.GetString(Constants.YesButton);
            string secondaryButton = stringsResourceLoader.GetString(Constants.CancelButton);
            string result;
            string errorContent;
            string errorTitle;
            var parents = new Collection<string>()
            {
                currentFolderId.Substring(1, currentFolderId.Length - 2)
            };
            var contentDialog = CreateInputContentDialog(dialogTitle, placeHolder, inputText, primaryButton, secondaryButton);
            var dialogResult = await contentDialog.ShowAsync();
            var gridItem = (ContentDialogControlViewModel)contentDialog.DataContext;
            var folderName = gridItem.InputText;
            folderName = ValidateItemName(folderName);
            if (dialogResult == ContentDialogResult.Primary && !string.IsNullOrEmpty(folderName))
            {
                if (DateTime.Now.Subtract(tokenResult.LastRefreshTime).Seconds >= int.Parse(tokenResult.ExpiresIn))
                {
                    await RefreshTokenAsync().ConfigureAwait(true);
                }                
                result = await googleDriveService.CreateNewFolderAsync(folderName, parents, tokenResult.AccessToken).ConfigureAwait(true);
                if (result == Constants.Success)
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
            string inputText = selectedGridItem.DisplayName;
            string primaryButton = stringsResourceLoader.GetString(Constants.YesButton);
            string secondaryButton = stringsResourceLoader.GetString(Constants.CancelButton);
            string result;
            if (selectedGridItem != null)
            {
                string errorContent;
                string errorTitle;
                var contentDialog = CreateInputContentDialog(dialogTitle, placeHolder, inputText, primaryButton, secondaryButton);
                var dialogResult = await contentDialog.ShowAsync();
                var gridItem = (ContentDialogControlViewModel)contentDialog.DataContext;
                var fileName = gridItem.InputText;
                fileName = ValidateItemName(fileName);
                
                if (dialogResult == ContentDialogResult.Primary && !string.IsNullOrEmpty(fileName))
                {
                    if (DateTime.Now.Subtract(tokenResult.LastRefreshTime).Seconds >= int.Parse(tokenResult.ExpiresIn))
                    {
                        await RefreshTokenAsync().ConfigureAwait(true);
                    }
                    result = await googleDriveService.RenameFileAsync(selectedGridItem.Id, fileName, tokenResult.AccessToken).ConfigureAwait(true);
                    if (result == Constants.Success)
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
