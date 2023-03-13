using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FileManager.Commands;
using System.Windows.Input;
using Windows.UI.Xaml.Controls;
using System.Net.Http;
using Google.Apis.Auth.OAuth2;
using static System.Net.WebRequestMethods;
using System.Net.Http.Headers;
using Windows.UI.Popups;
using Windows.Data.Json;
using FileManager.Models;
using System.Collections.ObjectModel;
using Windows.ApplicationModel.Resources;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using Windows.UI;

namespace FileManager.ViewModels
{
    public class GoogleDriveViewModel : BindableBase
    {
        private const string authEndpoint = "https://oauth2.googleapis.com/token";
        private const string scope = "https://www.googleapis.com/auth/drive";
        private const string clientId = "148004585806-56makt200p390b18n2q21ugq71tb2msg.apps.googleusercontent.com";
        private const string secret = "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";
        private const string googleUri = "https://accounts.google.com/o/oauth2/auth?client_id=" + clientId + "&redirect_uri=" + "https://localhost/" + "&response_type=code&scope=" + scope;
        private Uri webViewCurrentSource;
        private bool isWebViewVisible;
        private bool isContentVisible;
        private DateTime lastRefreshTime;
        private Collection<FileControlViewModel> storageFiles;
        private FileControlViewModel selectedGridItem;
        private TokenResult tokenResult;
        private ResourceLoader themeResourceLoader;
        private ICommand navigationStartingCommand;

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

        public GoogleDriveViewModel()
        {
            tokenResult = new TokenResult();
            ChangeColorMode(settings, this);
            NavigationStartingCommand = new RelayCommand(NavigationStarting);
            IsWebViewVisible = true;
            WebViewCurrentSource = new Uri(googleUri);
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
            if (webView != null )
            {
                if (webView.Uri.ToString().StartsWith("https://localhost/", StringComparison.Ordinal))
                {
                    string navigationUri = webView.Uri.ToString();
                    if (navigationUri.Contains("code=", StringComparison.Ordinal))
                    {
                        string exchangeCode = navigationUri.Substring(navigationUri.IndexOf('=') + 1, navigationUri.IndexOf('&') - navigationUri.IndexOf('=') - 1);
                        webView.Cancel = true;                        
                        IsWebViewVisible = false;
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

        private async Task GetItemsAsync()
        {
            const string image = "image";
            const string photo = "photo";
            const string shortcut = "shortcut";
            const string video = "video";
            const string audio = "audio";
            const string folder = "folder";
            const string file = "file";
            const string mimeType = "mimeType";

            using (HttpClient client = new HttpClient())
            {
                if (DateTime.Now.Subtract(lastRefreshTime).Seconds >= int.Parse(tokenResult.ExpiresIn))
                {
                    await RefreshTokenAsync().ConfigureAwait(true);
                }
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(tokenResult.TokenType, tokenResult.AccessToken);

                var files = await client.GetAsync("https://www.googleapis.com/drive/v3/files").ConfigureAwait(true);
                string responseFiles = await files.Content.ReadAsStringAsync().ConfigureAwait(true);

                List<FileControlViewModel> driveFiles = new List<FileControlViewModel>();
                foreach (var driveFile in JsonObject.Parse(responseFiles)["files"].GetArray())
                {
                    FileControlViewModel viewModel = new FileControlViewModel();
                    var type = JsonObject.Parse(driveFile.ToString())[mimeType].ToString();
                    var currentFile = JsonObject.Parse(driveFile.ToString());

                    if (type.Contains("." + folder, StringComparison.Ordinal))
                    {
                        viewModel = new FileControlViewModel() { Id = currentFile["id"].ToString(), Image = themeResourceLoader.GetString(folder), DisplayName = currentFile["name"].ToString(), Path = string.Empty, Type = folder };
                    }
                    else if (type.Contains("." + photo, StringComparison.Ordinal) || type.Contains("." + shortcut, StringComparison.Ordinal))
                    {
                        viewModel = new FileControlViewModel() { Id = currentFile["id"].ToString(), Image = themeResourceLoader.GetString(image), DisplayName = currentFile["name"].ToString(), Path = string.Empty, Type = image };
                    }
                    else if (type.Contains("." + video, StringComparison.Ordinal))
                    {
                        viewModel = new FileControlViewModel() { Id = currentFile["id"].ToString(), Image = themeResourceLoader.GetString(video), DisplayName = currentFile["name"].ToString(), Path = string.Empty, Type = video };
                    }
                    else if (type.Contains("." + audio, StringComparison.Ordinal))
                    {
                        viewModel = new FileControlViewModel() { Id = currentFile["id"].ToString(), Image = themeResourceLoader.GetString(audio), DisplayName = currentFile["name"].ToString(), Path = string.Empty, Type = audio };
                    }
                    else
                    {
                        viewModel = new FileControlViewModel() { Id = currentFile["id"].ToString(), Image = themeResourceLoader.GetString(file), DisplayName = currentFile["name"].ToString(), Path = string.Empty, Type = file };
                    }

                    driveFiles.Add(viewModel);
                }
                StorageFiles = new Collection<FileControlViewModel>(driveFiles.OrderBy(f => f.Type).ToList());
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
    }
}
