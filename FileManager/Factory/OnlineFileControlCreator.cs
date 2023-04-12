using FileManager.Helpers;
using FileManager.ViewModels.OnlineFileControls;
using System.Collections.Generic;
using System.IO;
using Windows.ApplicationModel.Resources;

namespace FileManager.Factory
{
    public static class OnlineFileControlCreator
    {
        private static readonly Dictionary<string, string> knownTypes = new Dictionary<string, string>()
            {
                { ".jpg", Constants.Image },
                { ".jpeg", Constants.Image },
                { ".jfif", Constants.Image },
                { ".gif", Constants.Image },
                { ".png", Constants.Image },
                { ".mp4", Constants.Video },
                { ".avi", Constants.Video },
                { ".wmv", Constants.Video },
                { ".amv", Constants.Video },
                { ".mp3", Constants.Audio },
                { ".ogg", Constants.Audio },
                { ".wma", Constants.Audio },
                { ".wav", Constants.Audio },
                { "application/vnd.google-apps.audio", Constants.Audio },
                { "application/vnd.google-apps.video", Constants.Video },
                { "application/vnd.google-apps.drawing", Constants.Image },
                { "application/vnd.google-apps.folder", Constants.Folder },
                { "application/vnd.google-apps.photo", Constants.Image },
                { "application/vnd.google-apps.shortcut", Constants.Image },
                { "image/jpeg", Constants.Image },
                { "video/x-ms-wmv", Constants.Video },
                { "video/mp4", Constants.Video },
                { "image/png", Constants.Image },
                { "drwxr-xr-x", Constants.Folder },
            };

        public static OnlineFileControlViewModel CreateFileControl(ResourceLoader themeResourceLoader, string id, string name, string type, string path = "")
        {
            OnlineFileControlViewModel fileControl;
            knownTypes.TryGetValue(type, out string controlType);
            switch (controlType)
            {
                case Constants.Image:
                    fileControl = new ImageFileControlViewModel(themeResourceLoader);
                    break;
                case Constants.Video:
                    fileControl = new VideoFileControlViewModel(themeResourceLoader);
                    break;
                case Constants.Audio:
                    fileControl = new AudioFileControlViewModel(themeResourceLoader);
                    break;
                case Constants.Folder:
                    fileControl = new FolderFileControlViewModel(themeResourceLoader);
                    break;
                default:
                    fileControl = new DocumentFileControlViewModel(themeResourceLoader);
                    controlType = Constants.File;
                    break;
            }
            fileControl.Id = id;
            fileControl.DisplayName = name;
            fileControl.Path = path;
            return fileControl;
        }
    }
}
