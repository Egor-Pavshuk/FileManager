using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using static Google.Apis.Drive.v3.FilesResource;
using FileManager.Helpers;
using FileManager.Models;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using Newtonsoft.Json;

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
                        var token = JsonConvert.DeserializeObject<TokenResult>(responseContent);
                        accessToken.Access_token = token.Access_token;
                        accessToken.Refresh_token = token.Refresh_token;
                        accessToken.Token_type = token.Token_type;
                        accessToken.Expires_in = token.Expires_in;
                        accessToken.Scope = token.Scope;
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
        public async Task<List<GoogleDriveFile>> GetFilesAsync(string q, string tokenType, string accessToken)
        {
            List<GoogleDriveFile> driveFiles = new List<GoogleDriveFile>();
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
                    var jsonResult = JsonConvert.DeserializeObject<DriveResult>(result);
                    nextPageToken = jsonResult.NextPageToken;
                    driveFiles.AddRange(jsonResult.Files);

                } while (!string.IsNullOrEmpty(nextPageToken));
            }
            return driveFiles;
        }
        public async Task<string> GetRootFolderIdAsync(string tokenType, string accessToken)
        {
            string result;
            using (HttpClient client = new HttpClient())
            {
                try
                {
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(tokenType, accessToken);
                    var rootFolderResult = await client.GetAsync($"https://www.googleapis.com/drive/v3/files/root").ConfigureAwait(true);
                    var rootFolderString = await rootFolderResult.Content.ReadAsStringAsync().ConfigureAwait(true);
                    result = JsonConvert.DeserializeObject<GoogleDriveFile>(rootFolderString).Id;
                }
                catch (HttpRequestException)
                {
                    result = Constants.Failed;
                }
            }
            return result;
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
                    request = service.Files.Delete(fileId);
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
                    string request = string.Join('&', $"&client_id={clientId}", $"client_secret={secret}", "grant_type=refresh_token", $"refresh_token={accessToken.Refresh_token}");
                    StringContent content = new StringContent(request, Encoding.UTF8, Constants.UrlencodedContentType);
                    try
                    {
                        response = await client.PostAsync(Constants.AuthEndpoint, content).ConfigureAwait(true);
                        string responseContent = await response.Content.ReadAsStringAsync().ConfigureAwait(true);
                        var token = JsonConvert.DeserializeObject<TokenResult>(responseContent);
                        accessToken.Access_token = token.Access_token;
                        accessToken.Expires_in = token.Expires_in;
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
