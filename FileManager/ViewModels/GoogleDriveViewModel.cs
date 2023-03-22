﻿using FileManager.Commands;
using FileManager.Models;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Download;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.ApplicationModel.Core;
using Windows.ApplicationModel.Resources;
using Windows.Data.Json;
using Windows.Media.Protection.PlayReady;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Pickers.Provider;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Popups;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;

namespace FileManager.ViewModels
{
    public class GoogleDriveViewModel : BindableBase
    {
        private const string authEndpoint = "https://oauth2.googleapis.com/token";
        private const string scope = "https://www.googleapis.com/auth/drive";
        private const string clientId = "148004585806-56makt200p390b18n2q21ugq71tb2msg.apps.googleusercontent.com";
        private const string secret = "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";
        private const string googleUri = "https://accounts.google.com/o/oauth2/auth?client_id=" + clientId + "&redirect_uri=" + "https://localhost/" + "&response_type=code&scope=" + scope;
        private string loadingText;
        private string currentFolderId;
        private Uri webViewCurrentSource;
        private bool isWebViewVisible;
        private bool isContentVisible;
        private bool isLoadingVisible;
        private bool isFilesVisible;
        private bool isBackButtonAvailable;
        private Stack<string> openedFoldersId = new Stack<string>();
        private DateTime lastRefreshTime;
        private Collection<GoogleFileControlViewModel> storageFiles;
        private List<string> downloadingFilesId;
        private GoogleFileControlViewModel selectedGridItem;
        private TokenResult tokenResult;
        private ResourceLoader themeResourceLoader;
        private ICommand navigationStartingCommand;
        private ICommand doubleClickedCommand;
        private ICommand getParentCommand;
        private ICommand downloadFileCommand;
        private ICommand uploadFileCommand;

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
        public bool IsContentVisible
        {
            get => isContentVisible;
            set
            {
                if (isContentVisible != value)
                {
                    isContentVisible = value;
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
        public GoogleFileControlViewModel SelectedGridItem
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
        public Collection<GoogleFileControlViewModel> StorageFiles
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

        public GoogleDriveViewModel()
        {
            tokenResult = new TokenResult();
            ChangeColorMode(settings, this);
            downloadingFilesId = new List<string>();
            NavigationStartingCommand = new RelayCommand(NavigationStarting);
            DoubleClickedCommand = new RelayCommand(OpenFolder);
            GetParentCommand = new RelayCommand(GetParent);
            DownloadFileCommand = new RelayCommand(DownloadFileAsync);
            UploadFileCommand = new RelayCommand(UploadFileAsync);
            IsWebViewVisible = true;
            WebViewCurrentSource = new Uri(googleUri);
            LoadingText = "Loading..."; //todo move to strings
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
                        IsContentVisible = true;
                        await ExchangeCodeOnTokenAsync(exchangeCode).ConfigureAwait(true);
                        await GetItemsAsync().ConfigureAwait(true);
                    }
                    else
                    {
                        var message = new MessageDialog("The response returned error!");
                        await message.ShowAsync();
                    }
                }
            }
        }

        private async Task ExchangeCodeOnTokenAsync(string exchangeCode)
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    string request = new StringBuilder()
                        .Append($"code={exchangeCode}")
                        .Append($"&client_id={clientId}")
                        .Append($"&client_secret={secret}")
                        .Append("&redirect_uri=https://localhost/")
                        .Append("&grant_type=authorization_code")
                        .Append("&access_type=offline")
                        .ToString();
                    StringContent content = new StringContent(request, Encoding.UTF8, "application/x-www-form-urlencoded");
                    var response = await client.PostAsync(authEndpoint, content).ConfigureAwait(true);

                    if (response.IsSuccessStatusCode)
                    {
                        string responseContent = await response.Content.ReadAsStringAsync().ConfigureAwait(true);
                        JsonObject responseObject = JsonObject.Parse(responseContent);
                        tokenResult.AccessToken = responseObject.GetNamedString("access_token");
                        tokenResult.RefreshToken = responseObject.GetNamedString("refresh_token");
                        tokenResult.TokenType = responseObject.GetNamedString("token_type");
                        tokenResult.ExpiresIn = responseObject.GetNamedValue("expires_in").ToString();
                        tokenResult.Scope = responseObject.GetNamedString("scope");
                        lastRefreshTime = DateTime.Now;
                    }
                    else
                    {
                        var message = new MessageDialog("The response returned error!");
                        await message.ShowAsync();
                    }
                    content.Dispose();
                }
            }
            catch (Exception)
            {

                throw; //todo exceptions
            }
            
        }

        private async Task GetItemsAsync(string q = "")
        {
            const string image = "image";
            const string photo = "photo";
            const string shortcut = "shortcut";
            const string video = "video";
            const string audio = "audio";
            const string folder = "folder";
            const string file = "file";
            const string mimeType = "mimeType";

            IsFilesVisible = false;
            IsLoadingVisible = true;

            JsonArray responseFiles = await GetFilesFromGoogleDriveAsync(q).ConfigureAwait(true);

            List<GoogleFileControlViewModel> driveFiles = new List<GoogleFileControlViewModel>();

            foreach (var driveFile in responseFiles)
            {
                GoogleFileControlViewModel viewModel;
                var currentFile = JsonObject.Parse(driveFile.Stringify());
                var type = currentFile[mimeType].ToString();

                if (type.Contains("." + folder, StringComparison.Ordinal))
                {
                    string currentFileName = currentFile["name"].ToString();
                    viewModel = new GoogleFileControlViewModel() { Id = currentFile["id"].ToString(), Image = themeResourceLoader.GetString(folder), DisplayName = currentFileName.Substring(1, currentFileName.Length - 2), Type = folder };
                    driveFiles.Add(viewModel);
                }
            }

            foreach (var driveFile in responseFiles)
            {
                GoogleFileControlViewModel viewModel;
                var currentFile = JsonObject.Parse(driveFile.Stringify());
                var type = currentFile[mimeType].ToString();
                string currentFileName = currentFile["name"].ToString();

                if (type.Contains("." + folder, StringComparison.Ordinal))
                {
                    continue;
                }

                if (type.Contains("." + photo, StringComparison.Ordinal) || type.Contains("." + shortcut, StringComparison.Ordinal) || type.Contains(photo, StringComparison.Ordinal) || type.Contains(image, StringComparison.Ordinal))
                {
                    viewModel = new GoogleFileControlViewModel() { Id = currentFile["id"].ToString(), Image = themeResourceLoader.GetString(image), DisplayName = currentFileName.Substring(1, currentFileName.Length - 2), Type = image };
                }
                else if (type.Contains("." + video, StringComparison.Ordinal) || type.Contains(video, StringComparison.Ordinal))
                {
                    viewModel = new GoogleFileControlViewModel() { Id = currentFile["id"].ToString(), Image = themeResourceLoader.GetString(video), DisplayName = currentFileName.Substring(1, currentFileName.Length - 2), Type = video };
                }
                else if (type.Contains("." + audio, StringComparison.Ordinal) || type.Contains(audio, StringComparison.Ordinal))
                {
                    viewModel = new GoogleFileControlViewModel() { Id = currentFile["id"].ToString(), Image = themeResourceLoader.GetString(audio), DisplayName = currentFileName.Substring(1, currentFileName.Length - 2), Type = audio };
                }
                else
                {
                    viewModel = new GoogleFileControlViewModel() { Id = currentFile["id"].ToString(), Image = themeResourceLoader.GetString(file), DisplayName = currentFileName.Substring(1, currentFileName.Length - 2), Type = file };
                }
                driveFiles.Add(viewModel);
            }

            StorageFiles = new Collection<GoogleFileControlViewModel>(driveFiles);
            CheckFilesForDownloading();
            IsLoadingVisible = false;
            IsFilesVisible = true;
        }

        private async Task<JsonArray> GetFilesFromGoogleDriveAsync(string q)
        {
            JsonArray driveFiles = new JsonArray();
            string nextPageToken = string.Empty;
            HttpResponseMessage driveResult;

            using (HttpClient client = new HttpClient())
            {
                if (DateTime.Now.Subtract(lastRefreshTime).Seconds >= int.Parse(tokenResult.ExpiresIn))
                {
                    await RefreshTokenAsync().ConfigureAwait(true);
                }
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(tokenResult.TokenType, tokenResult.AccessToken);

                if (string.IsNullOrEmpty(q))
                {
                    var rootFolderResult = await client.GetAsync($"https://www.googleapis.com/drive/v3/files/root").ConfigureAwait(true);
                    var rootFolderString = await rootFolderResult.Content.ReadAsStringAsync().ConfigureAwait(true);
                    string rootFolderId = JsonObject.Parse(rootFolderString)["id"].ToString();
                    currentFolderId = rootFolderId;
                    q = $"{rootFolderId}+in+parents and trashed=false&fields=files(id,+mimeType,+name)";
                }

                do
                {
                    driveResult = await client.GetAsync($"https://www.googleapis.com/drive/v3/files?pageToken={nextPageToken}&q={q}").ConfigureAwait(true);
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
                    file.DownloadStatus = "Downloading";
                }
            }
        }

        private async Task RefreshTokenAsync()
        {
            using (HttpClient client = new HttpClient())
            {
                string request = new StringBuilder()
                    .Append($"&client_id={clientId}")
                    .Append($"&client_secret={secret}")
                    .Append("&grant_type=refresh_token")
                    .Append($"&refresh_token={tokenResult.RefreshToken}")
                    .ToString();
                StringContent content = new StringContent(request, Encoding.UTF8, "application/x-www-form-urlencoded");
                var response = await client.PostAsync(authEndpoint, content).ConfigureAwait(true);

                if (response.IsSuccessStatusCode)
                {
                    string responseContent = await response.Content.ReadAsStringAsync().ConfigureAwait(true);
                    JsonObject responseObject = JsonObject.Parse(responseContent);
                    tokenResult.AccessToken = responseObject.GetNamedString("access_token");
                    tokenResult.ExpiresIn = responseObject.GetNamedValue("expires_in").ToString();
                    lastRefreshTime = DateTime.Now;
                }
                else
                {
                    var message = new MessageDialog("The response returned error!");
                    await message.ShowAsync();
                }
                content.Dispose();
            }
        }

        private void OpenFolder(object sender)
        {
            if (sender != null)
            {
                var gridItems = (GridView)sender;
                if (gridItems.SelectedItem is GoogleFileControlViewModel selectedItem && !string.IsNullOrEmpty(selectedItem.DisplayName) && selectedItem.Type == "folder")
                {
                    if (openedFoldersId.Count == 0)
                    {
                        IsBackButtonAvailable = true;
                    }
                    openedFoldersId.Push(currentFolderId);
                    currentFolderId = selectedItem.Id;
                    GetItemsAsync($"{selectedItem.Id}+in+parents and trashed=false&fields=files(*)").ConfigureAwait(true);
                }
            }

        }

        private void GetParent(object sender)
        {
            if (openedFoldersId.Count != 0)
            {
                string parentFolderId = openedFoldersId.Pop();
                currentFolderId = parentFolderId;
                GetItemsAsync($"{parentFolderId}+in+parents and trashed=false&fields=files(*)").ConfigureAwait(true);
                if (openedFoldersId.Count == 0)
                {
                    IsBackButtonAvailable = false;
                }
            }
        }

        private async void DownloadFileAsync(object sender)
        {
            const string googleDownloadUri = "https://www.googleapis.com/drive/v3/files/";
            const string folder = "folder";
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
                    if (DateTime.Now.Subtract(lastRefreshTime).Seconds >= int.Parse(tokenResult.ExpiresIn))
                    {
                        await RefreshTokenAsync().ConfigureAwait(true);
                    }

                    Uri source = new Uri(googleDownloadUri + selectedGridItem.Id.Substring(1, selectedGridItem.Id.Length - 2) + "?alt=media");

                    StorageFile destinationFile = await downloadFolder.CreateFileAsync(
                        selectedGridItem.DisplayName, CreationCollisionOption.GenerateUniqueName);

                    SelectedGridItem.IsDownloading = true;
                    SelectedGridItem.DownloadStatus = "Downloading"; //todo resources
                    downloadingFilesId.Add(SelectedGridItem.Id);
                    DownloadDriveFileAsync(source, destinationFile, SelectedGridItem.Id);


                    //BackgroundDownloader downloader = new BackgroundDownloader();
                    //downloader.SetRequestHeader("Authorization", tokenResult.AccessToken);
                    //DownloadOperation download = downloader.CreateDownload(source, destinationFile);
                    //download.IsRandomAccessRequired = true;
                    //Progress<DownloadOperation> progress = new Progress<DownloadOperation>(x => SelectedGridItem.ProgressChanged(download));
                    //cancellationToken = new CancellationTokenSource();

                    //try
                    //{
                    //    
                    //    await download.StartAsync().AsTask(cancellationToken.Token, progress).ConfigureAwait(true);                        
                    //}
                    //catch (TaskCanceledException)
                    //{
                    //    MessageDialog messageDialog = new MessageDialog("Task canceled!");
                    //    await messageDialog.ShowAsync();
                    //}
                }
            }
        }

        private async void DownloadDriveFileAsync(Uri source, StorageFile destinationFile, string fileId)
        {
            var downloadingFile = storageFiles.First(f => f.Id == fileId);
            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(tokenResult.TokenType, tokenResult.AccessToken);
                var stream = await client.GetStreamAsync(source).ConfigureAwait(true);
                using (var fileStream = await destinationFile.OpenStreamForWriteAsync().ConfigureAwait(true))
                {
                    await stream.CopyToAsync(fileStream).ConfigureAwait(true);
                }
            }
            downloadingFilesId.Remove(fileId);
            if (storageFiles.Any(f => f.Id == fileId))
            {
                downloadingFile = storageFiles.First(f => f.Id == fileId);
                downloadingFile.DownloadStatus = "Completed";//todo resources
                CloseDownloadingAsync(downloadingFile);
            }                 
        }
        private async void CloseDownloadingAsync(GoogleFileControlViewModel file)
        {
            await Task.Delay(2000).ConfigureAwait(true);
            file.IsDownloading = false;
            file.DownloadStatus = string.Empty;
        }

        private async void UploadFileAsync(object sender)
        {
            //const string googleUploadUri = "https://www.googleapis.com/upload/drive/v3/files";
            const string folder = "folder";

            if (selectedGridItem != null && !string.IsNullOrEmpty(selectedGridItem.DisplayName) && selectedGridItem.Type != folder)
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
                    if (DateTime.Now.Subtract(lastRefreshTime).Seconds >= int.Parse(tokenResult.ExpiresIn))
                    {
                        await RefreshTokenAsync().ConfigureAwait(true);
                    }

                    //Uri source = new Uri(googleUploadUri + $"?uploadType=resumable");

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

                        var request = service.Files.Create(fileMetadata, stream, "application/octet-stream");
                        request.Fields = "*";
                        var results = await request.UploadAsync().ConfigureAwait(true);

                        if (results.Status == Google.Apis.Upload.UploadStatus.Failed)
                        {
                            await new MessageDialog(results.Exception.Message).ShowAsync();
                        }
                        else
                        {
                            //var id = request.ResponseBody?.Id;
                            GetItemsAsync($"{currentFolderId}+in+parents&fields=files(*)").ConfigureAwait(true);
                        }
                    }                    
                }
                //var request = $"?uploadType=resumable&parents={currentFolderId}&";
                //StringContent content = new StringContent(request, Encoding.UTF8);

                //var stream = await uploadFile.OpenStreamForReadAsync().ConfigureAwait(true);
                //var bytes = new byte[(int)stream.Length];
                //stream.Read(bytes, 0, (int)stream.Length);

                //using (var content = new Windows.Web.Http.HttpMultipartContent())
                //{
                //    using (var fileContent = new ByteArrayContent(buffer.ToArray()))
                //    {
                //        var bufferContent = new Windows.Web.Http.HttpBufferContent(buffer);
                //        content.Add(bufferContent);
                //        //content.Add(new StringContent("charset=UTF-8"));
                //        content.Headers.ContentType = Windows.Web.Http.Headers.HttpMediaTypeHeaderValue.Parse("application/*");
                //        //content.Headers.ContentEncoding.Add(Windows.Web.Http.Headers.HttpContentCodingHeaderValue.Parse("UTF-8"));
                //        //content.Headers.ContentLength = lenght;

                //        var httpClient = new Windows.Web.Http.HttpClient();
                //        httpClient.DefaultRequestHeaders.Authorization = new Windows.Web.Http.Headers.HttpCredentialsHeaderValue(tokenResult.TokenType, tokenResult.AccessToken);
                //        try
                //        {
                //            var response = await httpClient.PostAsync(source, content);
                //            var result = JsonObject.Parse(await response.Content.ReadAsStringAsync());
                //        }
                //        catch (Exception e)
                //        {
                //            var exceptionText = e.InnerException;
                //        }


                //    }
                //}

                //using (var content = new MultipartFormDataContent())
                //{
                //    var streamContent = new StreamContent(stream);
                //    streamContent.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
                //    //content.Add(streamContent, "file", uploadFile.Name);
                //    //content.Headers.ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded");
                //    //content.Add(new StringContent("charset=UTF-8"));
                //    //JsonObject obj = new JsonObject
                //    //{
                //    //    { "parents", JsonValue.CreateStringValue($"[{currentFolderId}]") }
                //    //};
                //    //content.Headers.ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded");
                //    var request = new HttpRequestMessage(HttpMethod.Post, source);
                //    //request.Headers.Add("X-Upload-Content-Type", "application/octet-stream");
                //    //request.Headers.Add("X-Upload-Content-Length", streamContent.Headers.ContentLength.ToString());
                //    //request.Headers.Add("charset", "UTF - 8");

                //    //request.Headers.Add("parents", $"[{currentFolderId}]");

                //    request.Content = streamContent;
                //    request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
                //    request.Content.Headers.ContentLength = stream.Length;

                //    var fileContent = new ByteArrayContent(bytes);
                //    content.Add(fileContent);
                //    content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");

                //    //content.Add(streamContent, "file", uploadFile.Name);
                //    //content.Add(new ByteArrayContent(bytes));
                //    //content.Headers.ContentEncoding.Add(Windows.Web.Http.Headers.HttpContentCodingHeaderValue.Parse("UTF-8"));
                //    //content.Headers.ContentLength = streamContent.Headers.ContentLength;

                //    var httpClient = new HttpClient();
                //    httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(tokenResult.TokenType, tokenResult.AccessToken);
                //    try
                //    {
                //        var response = await httpClient.PostAsync(source, content).ConfigureAwait(true);
                //        var result = JsonObject.Parse(await response.Content.ReadAsStringAsync());
                //        //var location = response.Headers.Location.ToSting();
                //    }
                //    catch (Exception e)
                //    {
                //        var exceptionText = e.InnerException;
                //    }
            }

            
        }
    }
}
