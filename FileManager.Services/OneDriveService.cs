using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using ThirdPartyServices.Shared.Interfaces;
using ThirdPartyServices.Shared.Models;
using ThirdPartyServices.Shared.Models.Parameters;
using ThirdPartyServices.Shared.Models.Responses.Microsoft;
using Windows.Storage;
using Autofac;
using FileManager.Dependencies;
using FileManager.Helpers;
using FileManager.Models;
using Newtonsoft.Json.Linq;

namespace FileManager.Services
{
    public class OneDriveService
    {
        private readonly IOneDriveCloudService oneDriveCloudService;
        private readonly IMicrosoftAuthorizationService microsoftAuthService;
        public OneDriveService()
        {
            microsoftAuthService = VMDependencies.Container.Resolve<IMicrosoftAuthorizationService>();
            oneDriveCloudService = VMDependencies.Container.Resolve<IOneDriveCloudService>();
        }
        public async Task<Enums> CheckInternetConnectionAsync(string source)
        {
            Enums result;
            using (var client = new HttpClient())
            {
                try
                {
                    var responce = await client.GetAsync(source).ConfigureAwait(true);
                    result = Enums.Success;
                }
                catch (HttpRequestException)
                {
                    result = Enums.Failed;
                }
            }
            return result;
        }
        public async Task<Enums> OneDriveAuthAsync(MicrosoftAuthParams microsoftParams, TokenResult accessToken)
        {
            Enums result;
            try
            {
                var token = await microsoftAuthService.AuthorizeAsync(microsoftParams);
                accessToken.Access_token = token.Token;
                accessToken.Refresh_token = token.RefreshToken;
                accessToken.Expires_in = token.ExpiresIn.ToString();
                accessToken.LastRefreshTime = DateTime.Now;
                accessToken.Token_type = "Bearer";
                result = Enums.Success;
            }
            catch (Exception)
            {
                result = Enums.Failed;
            }
            return result;
        }
        public async Task<ItemsResponse> GetFilesAsync(string folderId, string accessToken)
        {
            ItemsResponse items;
            try
            {
                items = await oneDriveCloudService.GetFilesByFolderIdAsync(accessToken, folderId);
            }
            catch
            {
                items = null;
            }
            return items;
        }
        public async Task<Enums> DownloadFileAsync(StorageFolder destinationFolder, string fileName, string fileId, string accessToken)
        {
            Enums result = Enums.Failed;
            if (destinationFolder != null)
            {
                StorageFile destinationFile = await destinationFolder.CreateFileAsync(
                        fileName, CreationCollisionOption.GenerateUniqueName);

                var file = await oneDriveCloudService.DownloadItemAsync(new DownloadParams
                {
                    Token = accessToken,
                    FileName = fileId,
                });
                if (file.Name != null)
                {
                    try
                    {
                        await FileIO.WriteBytesAsync(destinationFile, (file.StreamContent as MemoryStream).ToArray());
                        result = Enums.Success;
                    }   
                    catch (Exception)
                    {
                        await destinationFile.DeleteAsync();
                        result = Enums.Failed;
                    }
                }
                else
                {
                    await destinationFile.DeleteAsync();
                    result = Enums.Failed;
                }
            }
            return result;
        }
        public async Task<Enums> UploadFileAsync(StorageFile uploadFile, string accessToken)
        {
            Enums result = Enums.Failed;

            if (!string.IsNullOrEmpty(accessToken) && uploadFile != null)
            {
                var streamData = await uploadFile.OpenReadAsync();
                var stream = streamData.AsStreamForRead();
                try
                {
                    await oneDriveCloudService.UploadItemAsync(stream, new UploadParams
                    {
                        Token = accessToken,
                        FileName = uploadFile.Name,
                    });
                    result = Enums.Success;
                }
                catch (Exception)
                {
                    result = Enums.Failed;
                }
            }
            return result;
        }
        public async Task<Enums> DeleteFileAsync(string fileId, string accessToken)
        {
            Enums result = Enums.Failed;
            if (fileId != null)
            {
                try
                {
                    await oneDriveCloudService.DeleteFileAsync(accessToken, fileId);
                    result = Enums.Success;
                }
                catch (HttpRequestException)
                {
                    result = Enums.Failed;
                }
            }
            return result;
        }
        public async Task<Enums> CreateNewFolderAsync(string folderName, string parent, string tokenType, string accessToken)
        {
            Enums result;
            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(tokenType, accessToken);
                string source = $"https://graph.microsoft.com/v1.0/me/drive/items/{parent}/children";
                var folderObject = JObject.FromObject(new
                {
                    name = folderName,
                    folder = new { },
                });
                HttpContent content = new StringContent(folderObject.ToString(), Encoding.UTF8, "application/json");
                try
                {
                    HttpResponseMessage response = await client.PostAsync(source, content);
                    result = Enums.Success;
                }
                catch (HttpRequestException)
                {
                    result = Enums.Failed;
                }
                finally
                {
                    content.Dispose();
                }
            }
            return result;
        }
        public async Task<Enums> RenameFileAsync(string itemId, string fileName, string tokenType, string accessToken)
        {
            Enums result;
            string source = $"https://graph.microsoft.com/v1.0/me/drive/items/{itemId}";
            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(tokenType, accessToken);
                var folderObject = JObject.FromObject(new
                {
                    name = fileName,
                });
                HttpContent content = new StringContent(folderObject.ToString(), Encoding.UTF8, "application/json");
                var method = "PATCH";
                var httpVerb = new HttpMethod(method);
                HttpRequestMessage httpRequestMessage = new HttpRequestMessage(httpVerb, source)
                {
                    Content = content,
                };
                try
                {
                    HttpResponseMessage response = await client.SendAsync(httpRequestMessage);
                    result = Enums.Success;
                }
                catch (HttpRequestException)
                {
                    result = Enums.Failed;
                }
                finally
                {
                    content.Dispose();
                }
            }
            return result;
        }
        public async Task<Enums> RefreshTokenAsync(MicrosoftAuthParams microsoftParams, TokenResult accessToken)
        {
            Enums result = Enums.Failed;
            if (accessToken != null)
            {
                try
                {
                    var token = await microsoftAuthService.ExchangeRefreshTokenAsync(microsoftParams, accessToken.Refresh_token);
                    accessToken.Access_token = token.Result.AccessToken;
                    accessToken.Expires_in = token.Result.ExpiresIn.ToString();
                    accessToken.Refresh_token = token.Result.RefreshToken;
                    accessToken.LastRefreshTime = DateTime.Now;
                    result = Enums.Success;
                }
                catch (Exception)
                {
                    result = Enums.Failed;
                }
            }
            return result;
        }
    }
}
