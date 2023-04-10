using FileManager.Helpers;
using FileManager.Models;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Windows.Data.Json;
using Windows.Storage;
using static Google.Apis.Drive.v3.FilesResource;

namespace FileManager.Services
{
    public class GoogleDriveService
    {
        public async Task<string> CheckInternetConnectionAsync(string source)
        {
            string result;
            using (var client = new HttpClient())
            {
                try
                {
                    var responce = await client.GetAsync(source).ConfigureAwait(true);
                    result = Constants.Success;
                }
                catch (HttpRequestException)
                {
                    result = Constants.Failed;
                }
            }
            return result;
        }
        public async Task<string> ExchangeCodeOnTokenAsync(string exchangeCode, string clientId, string secret, TokenResult accessToken)
        {
            string result = Constants.Failed;
            using (HttpClient client = new HttpClient())
            {
                string request = string.Join('&', $"code={exchangeCode}", $"client_id={clientId}", $"client_secret={secret}",
                    "redirect_uri=https://localhost/", "grant_type=authorization_code", "access_type=offline");
                StringContent content = new StringContent(request, Encoding.UTF8, Constants.UrlencodedContentType);
                try
                {
                    if (accessToken != null)
                    {
                        var response = await client.PostAsync(Constants.AuthEndpoint, content).ConfigureAwait(true);
                        string responseContent = await response.Content.ReadAsStringAsync().ConfigureAwait(true);
                        JsonObject responseObject = JsonObject.Parse(responseContent);
                        accessToken.AccessToken = responseObject.GetNamedString(Constants.AccessToken);
                        accessToken.RefreshToken = responseObject.GetNamedString(Constants.RefreshToken);
                        accessToken.TokenType = responseObject.GetNamedString(Constants.TokenType);
                        accessToken.ExpiresIn = responseObject.GetNamedValue(Constants.ExpiresIn).ToString();
                        accessToken.Scope = responseObject.GetNamedString(Constants.Scope);
                        accessToken.LastRefreshTime = DateTime.Now;
                        result = Constants.Success;
                    }
                }
                catch (HttpRequestException)
                {
                    result = Constants.Failed;
                }
                finally
                {
                    content.Dispose();
                }
            }
            return result;
        }
        public async Task<JsonArray> GetFilesAsync(string q, string tokenType, string accessToken)
        {
            JsonArray driveFiles = new JsonArray();
            string nextPageToken = string.Empty;
            HttpResponseMessage driveResult;

            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(tokenType, accessToken);
                do
                {
                    try
                    {
                        driveResult = await client.GetAsync($"https://www.googleapis.com/drive/v3/files?pageToken={nextPageToken}&q={q}").ConfigureAwait(true);
                    }
                    catch (HttpRequestException)
                    {
                        driveFiles = null;
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
        public async Task<string> DownloadFileAsync(Uri source, StorageFolder destinationFolder, string fileName, string tokenType, string accessToken)
        {
            string result = Constants.Failed;
            if (destinationFolder != null)
            {
                StorageFile destinationFile = await destinationFolder.CreateFileAsync(
                        fileName, CreationCollisionOption.GenerateUniqueName);
                using (HttpClient client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(tokenType, accessToken);
                    Stream stream;
                    try
                    {
                        stream = await client.GetStreamAsync(source).ConfigureAwait(true);
                        using (var fileStream = await destinationFile.OpenStreamForWriteAsync().ConfigureAwait(true))
                        {
                            await stream.CopyToAsync(fileStream).ConfigureAwait(true);
                        }
                        result = Constants.Success;
                    }
                    catch (HttpRequestException)
                    {
                        result = Constants.Failed;
                        await destinationFile.DeleteAsync();
                    }
                }
            }
            return result;
        }
        public async Task<string> UploadFileAsync(StorageFile uploadFile, Collection<string> parents, string accessToken)
        {
            string result = Constants.Failed;
            var credentional = GoogleCredential.FromAccessToken(accessToken).CreateScoped(DriveService.Scope.Drive);
            using (var service = new DriveService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credentional
            }))
            {
                if (uploadFile != null)
                {
                    var fileMetadata = new Google.Apis.Drive.v3.Data.File()
                    {
                        Name = uploadFile.Name,
                        Parents = parents
                    };
                    var stream = await uploadFile.OpenStreamForReadAsync().ConfigureAwait(true);
                    var request = service.Files.Create(fileMetadata, stream, Constants.OctetContentType);
                    request.Fields = "*";
                    var results = await request.UploadAsync().ConfigureAwait(true);

                    if (results.Status != Google.Apis.Upload.UploadStatus.Failed)
                    {
                        result = Constants.Success;
                    }
                }
            }
            return result;
        }
        public async Task<string> DeleteFileAsync(string fileId, string accessToken)
        {
            string result = Constants.Failed;
            DeleteRequest request;
            var credentional = GoogleCredential.FromAccessToken(accessToken).CreateScoped(DriveService.Scope.Drive);
            using (var service = new DriveService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credentional
            }))
            {
                if (fileId != null)
                {
                    request = service.Files.Delete(fileId.Substring(1, fileId.Length - 2));
                    try
                    {
                        await request.ExecuteAsync().ConfigureAwait(true);
                        result = Constants.Success;
                    }
                    catch (HttpRequestException)
                    {
                        result = Constants.Failed;
                    }
                }
            }
            return result;
        }
        public async Task<string> CreateNewFolderAsync(string folderName, Collection<string> parents, string accessToken)
        {
            string result;
            var credentional = GoogleCredential.FromAccessToken(accessToken).CreateScoped(DriveService.Scope.Drive);
            using (var service = new DriveService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credentional
            }))
            {
                var fileMetadata = new Google.Apis.Drive.v3.Data.File()
                {
                    Name = folderName,
                    Parents = parents,
                    MimeType = Constants.DriveFolderContentType
                };

                try
                {
                    var request = service.Files.Create(fileMetadata);
                    request.Fields = "*";
                    await request.ExecuteAsync().ConfigureAwait(true);
                    result = Constants.Success;
                }
                catch (HttpRequestException)
                {
                    result = Constants.Failed;
                }
            }
            return result;
        }
        public async Task<string> RenameFileAsync(string itemId, string fileName, string accessToken)
        {
            string result = string.Empty;
            var credentional = GoogleCredential.FromAccessToken(accessToken).CreateScoped(DriveService.Scope.Drive);
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
                    if (itemId != null)
                    {
                        itemId = itemId.AsSpan().Slice(1, itemId.Length - 2).ToString();
                    }
                    var request = service.Files.Update(fileMetadata, itemId);
                    await request.ExecuteAsync().ConfigureAwait(true);
                    result = Constants.Success;
                }
                catch (HttpRequestException)
                {
                    result = Constants.Failed;
                }
            }
            return result;
        }
        public async Task<string> RefreshTokenAsync(string clientId, string secret, TokenResult accessToken)
        {
            HttpResponseMessage response;
            string result = string.Empty;
            using (HttpClient client = new HttpClient())
            {
                if (accessToken != null)
                {
                    string request = string.Join('&', $"&client_id={clientId}", $"client_secret={secret}", "grant_type=refresh_token", $"refresh_token={accessToken.RefreshToken}");
                    StringContent content = new StringContent(request, Encoding.UTF8, Constants.UrlencodedContentType);
                    try
                    {
                        response = await client.PostAsync(Constants.AuthEndpoint, content).ConfigureAwait(true);
                        string responseContent = await response.Content.ReadAsStringAsync().ConfigureAwait(true);
                        JsonObject responseObject = JsonObject.Parse(responseContent);
                        accessToken.AccessToken = responseObject.GetNamedString(Constants.AccessToken);
                        accessToken.ExpiresIn = responseObject.GetNamedValue(Constants.ExpiresIn).ToString();
                        accessToken.LastRefreshTime = DateTime.Now;
                        result = Constants.Success;
                    }
                    catch (HttpRequestException)
                    {
                        result = Constants.Failed;
                    }
                    finally
                    {
                        content.Dispose();
                    }
                }
            }
            return result;
        }
    }
}
