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
            NavigationStartingCommand = new RelayCommand(NavigationStarting);
            IsWebViewVisible = true;
            WebViewCurrentSource = new Uri(googleUri);
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
            using (HttpClient client = new HttpClient())
            {
                if (DateTime.Now.Subtract(lastRefreshTime).Seconds >= int.Parse(tokenResult.ExpiresIn))
                {
                    await RefreshTokenAsync().ConfigureAwait(true);
                }
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(tokenResult.TokenType, tokenResult.AccessToken);

                var files = await client.GetAsync("https://www.googleapis.com/drive/v3/files").ConfigureAwait(true);
                string responseFiles = await files.Content.ReadAsStringAsync().ConfigureAwait(true);

            }
        }

        private async Task RefreshTokenAsync()
        {
            using (HttpClient client = new HttpClient())
            {
                string request = new StringBuilder()
                    .Append($"&client_id={clientId}")
                    .Append($"&client_secret={secret}")
                    .Append("&grant_type=authorization_code")
                    .Append($"&refresh_token={tokenResult.RefreshToken}")
                    .ToString();
                StringContent content = new StringContent(request, Encoding.UTF8, "application/x-www-form-urlencoded");
                var response = await client.PostAsync(authEndpoint, content).ConfigureAwait(true);

                if (response.IsSuccessStatusCode)
                {
                    string responseContent = await response.Content.ReadAsStringAsync().ConfigureAwait(true);
                    JsonObject responseObject = JsonObject.Parse(responseContent);
                    tokenResult.AccessToken = responseObject.GetNamedString("access_token");
                    tokenResult.RefreshToken = responseObject.GetNamedString("refresh_token");
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
