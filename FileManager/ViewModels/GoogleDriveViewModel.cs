﻿using FileManager.Commands;
using FileManager.Controlls;
using FileManager.Models;
using FileManager.Validation;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
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
        private const string AuthEndpoint = "https://oauth2.googleapis.com/token";
        private const string Scope = "https://www.googleapis.com/auth/drive";
        private const string ClientId = "148004585806-56makt200p390b18n2q21ugq71tb2msg.apps.googleusercontent.com";
        private const string Secret = "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";
        private const string GoogleUri = "https://accounts.google.com/o/oauth2/auth?client_id=" + ClientId + "&redirect_uri=" + "https://localhost/" + "&response_type=code&scope=" + Scope;
        private const string GoogleDownloadUri = "https://www.googleapis.com/drive/v3/files/";
        private const string Resources = "Resources";
        private const string Loading = "loadingText";
        private const string Image = "image";
        private const string Video = "video";
        private const string Audio = "audio";
        private const string Folder = "folder";
        private const string File = "file";
        private const string Photo = "photo";
        private const string Shortcut = "shortcut";
        private const string MimeType = "mimeType";
        private const string ImagesDark = "ImagesDark";
        private const string ImagesLight = "ImagesLight";
        private const string ConnectionError = "connectionError";
        private const string ResponseError = "responseError";
        private const string ConnectionErrorContent = "connectionErrorContent";
        private const string Failed = "failed";
        private const string AccessToken = "access_token";
        private const string ExpiresIn = "expires_in";
        private const string UrlencodedContentType = "application/x-www-form-urlencoded";
        private const string OctetContentType = "application/octet-stream";
        private const string DriveFolderContentType = "application/vnd.google-apps.folder";
        private const string DownloadingText = "downloadingText";
        private const string DownloadCompleted = "downloadCompleted";
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
        private readonly Stack<string> openedFoldersId = new Stack<string>();
        private DateTime lastRefreshTime;
        private Collection<OnlineFileControlViewModel> storageFiles;
        private readonly List<string> downloadingFilesId;
        private OnlineFileControlViewModel selectedGridItem;
        private readonly TokenResult tokenResult;
        private readonly ResourceLoader stringsResourceLoader;
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
            stringsResourceLoader = ResourceLoader.GetForCurrentView(Resources);
            IsWebViewVisible = true;
            _ = CheckInternetConnectionAsync();
            WebViewCurrentSource = new Uri(GoogleUri);
            LoadingText = stringsResourceLoader.GetString(Loading);
            ErrorText = stringsResourceLoader.GetString(ConnectionErrorContent);
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

        private async Task CheckInternetConnectionAsync()
        {
            using (var client = new HttpClient())
            {
                try
                {
                    var result = await client.GetAsync(GoogleUri).ConfigureAwait(true);
                }
                catch (HttpRequestException)
                {
                    IsErrorVisible = true;
                    IsCommandPanelVisible = false;
                    IsWebViewVisible = false;
                }
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
                        await new MessageDialog(stringsResourceLoader.GetString(ResponseError) + ".")
                        {
                            Title = stringsResourceLoader.GetString(Failed) + "!"
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
            using (HttpClient client = new HttpClient())
            {
                string request = new StringBuilder()
                    .Append($"code={exchangeCode}")
                    .Append($"&client_id={ClientId}")
                    .Append($"&client_secret={Secret}")
                    .Append("&redirect_uri=https://localhost/")
                    .Append("&grant_type=authorization_code")
                    .Append("&access_type=offline")
                    .ToString();
                StringContent content = new StringContent(request, Encoding.UTF8, UrlencodedContentType);

                try
                {
                    var response = await client.PostAsync(AuthEndpoint, content).ConfigureAwait(true);

                    if (response.IsSuccessStatusCode)
                    {
                        string responseContent = await response.Content.ReadAsStringAsync().ConfigureAwait(true);
                        JsonObject responseObject = JsonObject.Parse(responseContent);
                        tokenResult.AccessToken = responseObject.GetNamedString(AccessToken);
                        tokenResult.RefreshToken = responseObject.GetNamedString("refresh_token");
                        tokenResult.TokenType = responseObject.GetNamedString("token_type");
                        tokenResult.ExpiresIn = responseObject.GetNamedValue(ExpiresIn).ToString();
                        tokenResult.Scope = responseObject.GetNamedString("scope");
                        lastRefreshTime = DateTime.Now;
                    }
                    else
                    {
                        await new MessageDialog(stringsResourceLoader.GetString(ResponseError) + response.StatusCode)
                        {
                            Title = stringsResourceLoader.GetString(Failed) + "!"
                        }.ShowAsync();
                    }
                }
                catch (HttpRequestException)
                {
                    await new MessageDialog(stringsResourceLoader.GetString(ConnectionErrorContent))
                    {
                        Title = stringsResourceLoader.GetString(ConnectionError)
                    }.ShowAsync();
                }
                finally
                {
                    content.Dispose();
                }
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
                var type = currentFile[MimeType].ToString();
                if (!type.Contains("." + Folder, StringComparison.Ordinal))
                {
                    continue;
                }

                OnlineFileControlViewModel viewModel;              
                string currentFileName = currentFile["name"].ToString();
                viewModel = new OnlineFileControlViewModel() { Id = currentFile["id"].ToString(), Image = themeResourceLoader.GetString(Image), DisplayName = currentFileName.Substring(1, currentFileName.Length - 2), Type = Image };
                driveFiles.Add(viewModel);
            }

            foreach (var driveFile in responseFiles)
            {
                OnlineFileControlViewModel viewModel;
                var currentFile = JsonObject.Parse(driveFile.Stringify());
                var type = currentFile[MimeType].ToString();
                if (type.Contains("." + Folder, StringComparison.Ordinal))
                {
                    continue;
                }
                string currentFileName = currentFile["name"].ToString();

                if (type.Contains("." + Photo, StringComparison.Ordinal) || type.Contains("." + Shortcut, StringComparison.Ordinal) || type.Contains(Photo, StringComparison.Ordinal) || type.Contains(Image, StringComparison.Ordinal))
                {
                    viewModel = new OnlineFileControlViewModel() { Id = currentFile["id"].ToString(), Image = themeResourceLoader.GetString(Image), DisplayName = currentFileName.Substring(1, currentFileName.Length - 2), Type = Image };
                }
                else if (type.Contains("." + Video, StringComparison.Ordinal) || type.Contains(Video, StringComparison.Ordinal))
                {
                    viewModel = new OnlineFileControlViewModel() { Id = currentFile["id"].ToString(), Image = themeResourceLoader.GetString(Video), DisplayName = currentFileName.Substring(1, currentFileName.Length - 2), Type = Video };
                }
                else if (type.Contains("." + Audio, StringComparison.Ordinal) || type.Contains(Audio, StringComparison.Ordinal))
                {
                    viewModel = new OnlineFileControlViewModel() { Id = currentFile["id"].ToString(), Image = themeResourceLoader.GetString(Audio), DisplayName = currentFileName.Substring(1, currentFileName.Length - 2), Type = Audio };
                }
                else
                {
                    viewModel = new OnlineFileControlViewModel() { Id = currentFile["id"].ToString(), Image = themeResourceLoader.GetString(File), DisplayName = currentFileName.Substring(1, currentFileName.Length - 2), Type = File };
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
            JsonArray driveFiles = new JsonArray();
            string nextPageToken = string.Empty;
            HttpResponseMessage driveResult;
            string q;

            using (HttpClient client = new HttpClient())
            {
                if (DateTime.Now.Subtract(lastRefreshTime).Seconds >= int.Parse(tokenResult.ExpiresIn))
                {
                    await RefreshTokenAsync().ConfigureAwait(true);
                }
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(tokenResult.TokenType, tokenResult.AccessToken);

                if (string.IsNullOrEmpty(folderId))
                {
                    var rootFolderResult = await client.GetAsync($"https://www.googleapis.com/drive/v3/files/root").ConfigureAwait(true);
                    var rootFolderString = await rootFolderResult.Content.ReadAsStringAsync().ConfigureAwait(true);
                    string rootFolderId = JsonObject.Parse(rootFolderString)["id"].ToString();
                    currentFolderId = rootFolderId;
                    q = $"{rootFolderId}+in+parents and trashed=false&fields=files(id,+mimeType,+name)";
                }
                else
                {
                    q = $"{folderId}+in+parents and trashed=false&fields=files(*)";
                }

                do
                {
                    try
                    {
                        driveResult = await client.GetAsync($"https://www.googleapis.com/drive/v3/files?pageToken={nextPageToken}&q={q}").ConfigureAwait(true);
                    }
                    catch (HttpRequestException)
                    {
                        await new MessageDialog(stringsResourceLoader.GetString(ConnectionErrorContent))
                        {
                            Title = stringsResourceLoader.GetString(ConnectionError)
                        }.ShowAsync();
                        break;
                    }

                    var result = await driveResult.Content.ReadAsStringAsync().ConfigureAwait(true);
                    var jsonParse = JsonObject.Parse(result);
                    var jsonFiles = jsonParse["files"].GetArray();
                    foreach (var resultFile in jsonFiles)
                    {
                        driveFiles.Add(resultFile);
                    }
                    if (jsonParse.TryGetValue("nextPageToken", out IJsonValue value))
                    {
                        nextPageToken = value.ToString();
                        nextPageToken = nextPageToken.Substring(1, nextPageToken.Length - 2);
                    }
                    else
                    {
                        nextPageToken = string.Empty;
                    }

                } while (!string.IsNullOrEmpty(nextPageToken));
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
                    file.DownloadStatus = stringsResourceLoader.GetString(DownloadingText);
                }
            }
        }

        private async Task RefreshTokenAsync()
        {
            using (HttpClient client = new HttpClient())
            {
                string request = new StringBuilder()
                    .Append($"&client_id={ClientId}")
                    .Append($"&client_secret={Secret}")
                    .Append("&grant_type=refresh_token")
                    .Append($"&refresh_token={tokenResult.RefreshToken}")
                    .ToString();
                StringContent content = new StringContent(request, Encoding.UTF8, UrlencodedContentType);

                HttpResponseMessage response;
                try
                {
                    response = await client.PostAsync(AuthEndpoint, content).ConfigureAwait(true);

                    if (response.IsSuccessStatusCode)
                    {
                        string responseContent = await response.Content.ReadAsStringAsync().ConfigureAwait(true);
                        JsonObject responseObject = JsonObject.Parse(responseContent);
                        tokenResult.AccessToken = responseObject.GetNamedString(AccessToken);
                        tokenResult.ExpiresIn = responseObject.GetNamedValue(ExpiresIn).ToString();
                        lastRefreshTime = DateTime.Now;
                    }
                    else
                    {
                        await new MessageDialog(stringsResourceLoader.GetString(ResponseError) + response.StatusCode)
                        {
                            Title = stringsResourceLoader.GetString(Failed) + "!"
                        }.ShowAsync();
                    }
                }
                catch (HttpRequestException)
                {
                    await new MessageDialog(stringsResourceLoader.GetString(ConnectionErrorContent))
                    {
                        Title = stringsResourceLoader.GetString(ConnectionError)
                    }.ShowAsync();
                }
                finally
                {
                    content.Dispose();
                }
            }
        }

        private void OpenFolder(object sender)
        {
            if (sender != null)
            {
                var gridItems = (GridView)sender;
                if (gridItems.SelectedItem is OnlineFileControlViewModel selectedItem && !string.IsNullOrEmpty(selectedItem.DisplayName) && selectedItem.Type == "folder")
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
                    if (DateTime.Now.Subtract(lastRefreshTime).Seconds >= int.Parse(tokenResult.ExpiresIn))
                    {
                        await RefreshTokenAsync().ConfigureAwait(true);
                    }

                    Uri source = new Uri(GoogleDownloadUri + selectedGridItem.Id.Substring(1, selectedGridItem.Id.Length - 2) + "?alt=media");

                    StorageFile destinationFile = await downloadFolder.CreateFileAsync(
                        selectedGridItem.DisplayName, CreationCollisionOption.GenerateUniqueName);

                    SelectedGridItem.IsDownloading = true;
                    SelectedGridItem.DownloadStatus = stringsResourceLoader.GetString(DownloadingText);
                    downloadingFilesId.Add(SelectedGridItem.Id);
                    _ = DownloadDriveFileAsync(source, destinationFile, SelectedGridItem.Id);
                }
            }
        }

        private async Task DownloadDriveFileAsync(Uri source, StorageFile destinationFile, string fileId)
        {
            bool isDownloadSuccess = true;
            var downloadingFile = storageFiles.First(f => f.Id == fileId);
            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(tokenResult.TokenType, tokenResult.AccessToken);
                Stream stream;
                try
                {
                    stream = await client.GetStreamAsync(source).ConfigureAwait(true);
                    using (var fileStream = await destinationFile.OpenStreamForWriteAsync().ConfigureAwait(true))
                    {
                        await stream.CopyToAsync(fileStream).ConfigureAwait(true);
                    }
                }
                catch (HttpRequestException)
                {
                    await new MessageDialog(stringsResourceLoader.GetString(ConnectionErrorContent))
                    {
                        Title = stringsResourceLoader.GetString(ConnectionError)
                    }.ShowAsync();
                    isDownloadSuccess = false;
                    await destinationFile.DeleteAsync();
                }
            }
            downloadingFilesId.Remove(fileId);
            downloadingFile = storageFiles.FirstOrDefault(f => f.Id == fileId);
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
                UploadFileOnDriveAsync(uploadFile, currentFolderId);
            }
        }

        private async void UploadFileOnDriveAsync(StorageFile uploadFile, string folderId)
        {
            if (DateTime.Now.Subtract(lastRefreshTime).Seconds >= int.Parse(tokenResult.ExpiresIn))
            {
                await RefreshTokenAsync().ConfigureAwait(true);
            }

            var credentional = GoogleCredential.FromAccessToken(tokenResult.AccessToken).CreateScoped(DriveService.Scope.Drive);
            using (var service = new DriveService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credentional
            }))
            {
                var fileMetadata = new Google.Apis.Drive.v3.Data.File()
                {
                    Name = uploadFile.Name,
                    Parents = new List<string>() { currentFolderId.Substring(1, currentFolderId.Length - 2) }
                };

                var stream = await uploadFile.OpenStreamForReadAsync().ConfigureAwait(true);

                var request = service.Files.Create(fileMetadata, stream, OctetContentType);
                request.Fields = "*";
                var results = await request.UploadAsync().ConfigureAwait(true);

                if (results.Status == Google.Apis.Upload.UploadStatus.Failed)
                {
                    await new MessageDialog(stringsResourceLoader.GetString(ConnectionErrorContent))
                    {
                        Title = stringsResourceLoader.GetString(ConnectionError)
                    }.ShowAsync();
                }
                else if (currentFolderId == folderId)
                {
                    _ = GetItemsAsync(currentFolderId).ConfigureAwait(true);
                }
            }
        }

        private async void DeleteFileAsync(object sender)
        {
            string folderId = currentFolderId;
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
                    if (DateTime.Now.Subtract(lastRefreshTime).Seconds >= int.Parse(tokenResult.ExpiresIn))
                    {
                        await RefreshTokenAsync().ConfigureAwait(true);
                    }
                    var credentional = GoogleCredential.FromAccessToken(tokenResult.AccessToken).CreateScoped(DriveService.Scope.Drive);
                    using (var service = new DriveService(new BaseClientService.Initializer()
                    {
                        HttpClientInitializer = credentional
                    }))
                    {
                        var request = service.Files.Delete(selectedGridItem.Id.Substring(1, selectedGridItem.Id.Length - 2));
                        try
                        {
                            await request.ExecuteAsync().ConfigureAwait(true);
                            if (currentFolderId == folderId)
                            {
                                _ = GetItemsAsync(currentFolderId).ConfigureAwait(true);
                            }
                        }
                        catch (HttpRequestException)
                        {
                            await new MessageDialog(stringsResourceLoader.GetString(ConnectionErrorContent))
                            {
                                Title = stringsResourceLoader.GetString(ConnectionError)
                            }.ShowAsync();
                        }

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

                if (DateTime.Now.Subtract(lastRefreshTime).Seconds >= int.Parse(tokenResult.ExpiresIn))
                {
                    await RefreshTokenAsync().ConfigureAwait(true);
                }

                var credentional = GoogleCredential.FromAccessToken(tokenResult.AccessToken).CreateScoped(DriveService.Scope.Drive);
                using (var service = new DriveService(new BaseClientService.Initializer()
                {
                    HttpClientInitializer = credentional
                }))
                {
                    var fileMetadata = new Google.Apis.Drive.v3.Data.File()
                    {
                        Name = folderName,
                        Parents = new List<string>() { currentFolderId.Substring(1, currentFolderId.Length - 2) },
                        MimeType = DriveFolderContentType
                    };

                    try
                    {
                        var request = service.Files.Create(fileMetadata);
                        request.Fields = "*";
                        await request.ExecuteAsync().ConfigureAwait(true);
                        _ = GetItemsAsync(currentFolderId).ConfigureAwait(true);
                    }
                    catch (HttpRequestException)
                    {
                        await new MessageDialog(stringsResourceLoader.GetString(ConnectionErrorContent))
                        {
                            Title = stringsResourceLoader.GetString(ConnectionError)
                        }.ShowAsync();
                    }
                }
            }
            else if (result == ContentDialogResult.Primary && !ItemNameValidation.Validate(folderName))
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
                var fileName = gridItem.InputText;

                if (result == ContentDialogResult.Primary && ItemNameValidation.Validate(fileName))
                {
                    while (fileName.EndsWith(' '))
                    {
                        fileName = fileName.Remove(fileName.Length - 1);
                    }
                    while (fileName.StartsWith(' '))
                    {
                        fileName = fileName.Remove(0, 1);
                    }

                    if (DateTime.Now.Subtract(lastRefreshTime).Seconds >= int.Parse(tokenResult.ExpiresIn))
                    {
                        await RefreshTokenAsync().ConfigureAwait(true);
                    }

                    var credentional = GoogleCredential.FromAccessToken(tokenResult.AccessToken).CreateScoped(DriveService.Scope.Drive);
                    using (var service = new DriveService(new BaseClientService.Initializer()
                    {
                        HttpClientInitializer = credentional
                    }))
                    {
                        var fileMetadata = new Google.Apis.Drive.v3.Data.File()
                        {
                            Name = fileName,
                        };

                        try
                        {
                            var itemId = selectedGridItem.Id;
                            itemId = itemId.Substring(1, currentFolderId.Length - 2);
                            var request = service.Files.Update(fileMetadata, itemId);
                            await request.ExecuteAsync().ConfigureAwait(true);
                            _ = GetItemsAsync(currentFolderId).ConfigureAwait(true);
                        }
                        catch (HttpRequestException)
                        {
                            await new MessageDialog(stringsResourceLoader.GetString(ConnectionErrorContent))
                            {
                                Title = stringsResourceLoader.GetString(ConnectionError)
                            }.ShowAsync();
                        }
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
