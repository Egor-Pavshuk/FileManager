using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Windows.Storage;
using FileManager.Helpers;
using FileManager.Models;
using System.Text.RegularExpressions;

namespace FileManager.Services
{
    public class FtpService
    {
        public async Task<string> TryConnectAsync(string hostLink, string username, string password)
        {
            string connectionResult;
            try
            {
                FtpWebRequest request = (FtpWebRequest)WebRequest.Create(hostLink);
                request.Method = WebRequestMethods.Ftp.ListDirectory;
                request.Credentials = new NetworkCredential(username, password);
                var response = (FtpWebResponse)await request.GetResponseAsync().ConfigureAwait(true);
                response.Close();
                connectionResult = Enums.Success.ToString();
            }
            catch (WebException)
            {
                connectionResult = Enums.Failed.ToString();
            }
            catch (UriFormatException)
            {
                connectionResult = Constants.InvalidUriFormat;
            }

            return connectionResult;
        }
        public async Task<List<FtpFile>> GetFilesAsync(string path, string username, string password)
        {
            var fileName = path?.Split("/").Last();
            if (fileName.Contains("#", StringComparison.Ordinal))
            {
                var newFileName = fileName.Replace("#", "%23", StringComparison.Ordinal);
                path = path.Replace(fileName, newFileName, StringComparison.Ordinal);
            }
            FtpWebRequest request = (FtpWebRequest)WebRequest.Create(path);
            request.Method = WebRequestMethods.Ftp.ListDirectoryDetails;
            request.Credentials = new NetworkCredential(username, password);
            var response = (FtpWebResponse)await request.GetResponseAsync().ConfigureAwait(true);
            StreamReader streamReader = new StreamReader(response.GetResponseStream());
            List<FtpFile> files = new List<FtpFile>();
            string line = await streamReader.ReadLineAsync().ConfigureAwait(true);
            while (!string.IsNullOrEmpty(line))
            {
                files.Add(GetFtpFile(line));
                line = await streamReader.ReadLineAsync().ConfigureAwait(true);
            }
            streamReader.Close();
            response.Close();
            return files;
        }
        private FtpFile GetFtpFile(string line)
        {
            var elements = line.Split(" ").ToList();
            elements.RemoveAll(f => string.IsNullOrEmpty(f));
            int indexOfTime = elements.IndexOf(elements.FirstOrDefault(e => Regex.IsMatch(e, @"[0-2][0-9][:][0-5][0-9]")));
            var elementName = string.Join(" ", elements.GetRange(indexOfTime + 1, elements.Count - indexOfTime - 1));
            string fileType = elements[0];
            if (fileType != "drwxr-xr-x")
            {
                int startIndexOfExtension = elementName.LastIndexOf('.');
                if (startIndexOfExtension >= 0)
                {
                    fileType = elementName.Substring(startIndexOfExtension);
                }                
            }            
            string size = elements[indexOfTime - 3];
            string creationTime = string.Join(" ", elements[indexOfTime - 2], elements[indexOfTime - 1]);
            var ftpFile = new FtpFile()
            {
                Type = fileType,
                Name = elementName,
                Size = size,
                CreationTime = creationTime
            };
            return ftpFile;
        }
        public async Task<Enums> DownloadFileAsync(StorageFolder downloadFolder, string filePath, string username, string password)
        {
            Enums downloadingResult;
            Stream stream;
            string fileName = filePath?.Split('/').Last();
            StorageFile destinationFile = await downloadFolder?.CreateFileAsync(fileName, CreationCollisionOption.GenerateUniqueName);
            try
            {
                using (WebClient client = new WebClient())
                {
                    client.Credentials = new NetworkCredential(username, password);
                    if (fileName.Contains("#", StringComparison.Ordinal))
                    {
                        fileName = fileName.Replace("#", "%23", StringComparison.Ordinal);
                        filePath = filePath?.Replace(destinationFile.Name, fileName, StringComparison.Ordinal);
                    }
                    stream = await client.OpenReadTaskAsync(filePath).ConfigureAwait(true);
                }

                using (var fileStream = await destinationFile.OpenStreamForWriteAsync().ConfigureAwait(true))
                {
                    await stream.CopyToAsync(fileStream).ConfigureAwait(true);
                }
                downloadingResult = Enums.Success;
            }
            catch (WebException)
            {
                downloadingResult = Enums.Failed;
                await destinationFile?.DeleteAsync();
            }

            return downloadingResult;
        }
        public async Task<Enums> UploadFileAsync(StorageFile uploadFile, string destinationPath, string username, string password)
        {
            Enums uploadingResult;
            Stream responseStream;
            try
            {
                using (WebClient client = new WebClient())
                {
                    client.Credentials = new NetworkCredential(username, password);
                    responseStream = await client.OpenWriteTaskAsync(new Uri(destinationPath + "/" + uploadFile?.Name)).ConfigureAwait(true);
                }
                using (var fileStream = await uploadFile.OpenStreamForWriteAsync().ConfigureAwait(true))
                {
                    await fileStream.CopyToAsync(responseStream).ConfigureAwait(true);
                }
                uploadingResult = Enums.Success;
            }
            catch (WebException)
            {
                uploadingResult = Enums.Failed;
            }
            catch(FileLoadException)
            {
                uploadingResult = Enums.Failed;
            }
            return uploadingResult;
        }
        public async Task<Enums> DeleteFileAsync(string path, string type, string username, string password)
        {
            Enums result;
            try
            {
                FtpWebRequest request = (FtpWebRequest)WebRequest.Create(path);
                request.Credentials = new NetworkCredential(username, password);

                if (type == Constants.Folder)
                {
                    request.Method = WebRequestMethods.Ftp.RemoveDirectory;
                }
                else
                {
                    request.Method = WebRequestMethods.Ftp.DeleteFile;
                }

                FtpWebResponse response = (FtpWebResponse)await request.GetResponseAsync().ConfigureAwait(true);
                response.Close();
                result = Enums.Success;
            }
            catch (WebException)
            {
                result = Enums.Failed;
            }
            return result;
        }
        public async Task<Enums> CreateNewFolderAsync(string path, string folderName, string username, string password)
        {
            Enums creatingResult;
            try
            {
                FtpWebRequest request = (FtpWebRequest)WebRequest.Create(path + "/" + folderName);
                request.Credentials = new NetworkCredential(username, password);
                request.Method = WebRequestMethods.Ftp.MakeDirectory;
                FtpWebResponse response = (FtpWebResponse)await request.GetResponseAsync().ConfigureAwait(true);
                response.Close();

                creatingResult = Enums.Success;
            }
            catch (WebException)
            {
                creatingResult = Enums.Failed;
            }
            return creatingResult;
        }
        public async Task<Enums> RenameFileAsync(string path, string oldFileName, string newFileName, string username, string password)
        {
            Enums renameResult;
            try
            {
                FtpWebRequest request = (FtpWebRequest)WebRequest.Create(path + "/" + oldFileName);
                request.Credentials = new NetworkCredential(username, password);
                request.Method = WebRequestMethods.Ftp.Rename;
                request.RenameTo = newFileName;
                FtpWebResponse response = (FtpWebResponse)await request.GetResponseAsync().ConfigureAwait(true);
                response.Close();
                renameResult = Enums.Success;
            }
            catch (WebException)
            {
                renameResult = Enums.Failed;
            }
            return renameResult;
        }
    }
}
