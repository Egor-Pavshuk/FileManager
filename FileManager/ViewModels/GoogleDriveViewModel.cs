using FileManager.Commands;
using FileManager.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.ApplicationModel.Core;
using Windows.ApplicationModel.Resources;
using Windows.Data.Json;
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
        private Collection<FileControlViewModel> storageFiles;
        private FileControlViewModel selectedGridItem;
        private TokenResult tokenResult;
        private ResourceLoader themeResourceLoader;
        private ICommand navigationStartingCommand;
        private ICommand doubleClickedCommand;
        private ICommand getParentCommand;

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

        public GoogleDriveViewModel()
        {
            tokenResult = new TokenResult();
            ChangeColorMode(settings, this);
            NavigationStartingCommand = new RelayCommand(NavigationStarting);
            DoubleClickedCommand = new RelayCommand(OpenFolder);
            GetParentCommand = new RelayCommand(GetParent);
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

            using (HttpClient client = new HttpClient())
            {
                if (DateTime.Now.Subtract(lastRefreshTime).Seconds >= int.Parse(tokenResult.ExpiresIn))
                {
                    await RefreshTokenAsync().ConfigureAwait(true);
                }
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(tokenResult.TokenType, tokenResult.AccessToken);

                string nextPageToken = string.Empty;
                HttpResponseMessage driveResult;
                JsonArray responseFiles = new JsonArray();
                if (string.IsNullOrEmpty(q))
                {
                    var rootFolderResult = await client.GetAsync($"https://www.googleapis.com/drive/v3/files/root").ConfigureAwait(true);
                    var rootFolderString = await rootFolderResult.Content.ReadAsStringAsync().ConfigureAwait(true);
                    string rootFolderId = JsonObject.Parse(rootFolderString)["id"].ToString();
                    currentFolderId = rootFolderId;
                    q = $"{rootFolderId}+in+parents&fields=files(id,+mimeType,+name)";
                }

                do
                {
                    driveResult = await client.GetAsync($"https://www.googleapis.com/drive/v3/files?pageToken={nextPageToken}&q={q}").ConfigureAwait(true);
                    var result = await driveResult.Content.ReadAsStringAsync().ConfigureAwait(true);
                    var jsonParse = JsonObject.Parse(result);
                    var jsonFiles = jsonParse["files"].GetArray();
                    foreach (var resultFile in jsonFiles)
                    {
                        responseFiles.Add(resultFile);
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

                List<FileControlViewModel> driveFiles = new List<FileControlViewModel>();

                foreach (var driveFile in responseFiles)
                {
                    FileControlViewModel viewModel;
                    var currentFile = JsonObject.Parse(driveFile.Stringify());
                    var type = currentFile[mimeType].ToString();

                    if (type.Contains("." + folder, StringComparison.Ordinal))
                    {
                        string currentFileName = currentFile["name"].ToString();
                        viewModel = new FileControlViewModel() { Id = currentFile["id"].ToString(), Image = themeResourceLoader.GetString(folder), DisplayName = currentFileName.Substring(1, currentFileName.Length - 2), Path = string.Empty, Type = folder };
                        driveFiles.Add(viewModel);
                    }
                }

                foreach (var driveFile in responseFiles)
                {
                    FileControlViewModel viewModel;
                    var currentFile = JsonObject.Parse(driveFile.Stringify());
                    var type = currentFile[mimeType].ToString();
                    string currentFileName = currentFile["name"].ToString();

                    if (type.Contains("." + folder, StringComparison.Ordinal))
                    {
                        continue;
                    }

                    if (type.Contains("." + photo, StringComparison.Ordinal) || type.Contains("." + shortcut, StringComparison.Ordinal) || type.Contains(photo, StringComparison.Ordinal) || type.Contains(image, StringComparison.Ordinal))
                    {
                        viewModel = new FileControlViewModel() { Id = currentFile["id"].ToString(), Image = themeResourceLoader.GetString(image), DisplayName = currentFileName.Substring(1, currentFileName.Length - 2), Path = string.Empty, Type = image };
                    }
                    else if (type.Contains("." + video, StringComparison.Ordinal) || type.Contains(video, StringComparison.Ordinal))
                    {
                        viewModel = new FileControlViewModel() { Id = currentFile["id"].ToString(), Image = themeResourceLoader.GetString(video), DisplayName = currentFileName.Substring(1, currentFileName.Length - 2), Path = string.Empty, Type = video };
                    }
                    else if (type.Contains("." + audio, StringComparison.Ordinal) || type.Contains(audio, StringComparison.Ordinal))
                    {
                        viewModel = new FileControlViewModel() { Id = currentFile["id"].ToString(), Image = themeResourceLoader.GetString(audio), DisplayName = currentFileName.Substring(1, currentFileName.Length - 2), Path = string.Empty, Type = audio };
                    }
                    else
                    {
                        viewModel = new FileControlViewModel() { Id = currentFile["id"].ToString(), Image = themeResourceLoader.GetString(file), DisplayName = currentFileName.Substring(1, currentFileName.Length - 2), Path = string.Empty, Type = file };
                    }

                    driveFiles.Add(viewModel);
                }

                StorageFiles = new Collection<FileControlViewModel>(driveFiles);
                IsLoadingVisible = false;
                IsFilesVisible = true;
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
                if (gridItems.SelectedItem is FileControlViewModel selectedItem && !string.IsNullOrEmpty(selectedItem.DisplayName) && selectedItem.Type == "folder")
                {
                    if (openedFoldersId.Count == 0)
                    {
                        IsBackButtonAvailable = true;
                    }
                    openedFoldersId.Push(currentFolderId);
                    currentFolderId = selectedItem.Id;
                    GetItemsAsync($"{selectedItem.Id}+in+parents&fields=files(*)").ConfigureAwait(true);
                }
            }

        }

        private void GetParent(object sender)
        {
            if (openedFoldersId.Count != 0)
            {
                string parentFolderId = openedFoldersId.Pop();
                currentFolderId = parentFolderId;
                GetItemsAsync($"{parentFolderId}+in+parents&fields=files(*)").ConfigureAwait(true);
                if (openedFoldersId.Count == 0)
                {
                    IsBackButtonAvailable = false;
                }
            }
        }
    }
}
