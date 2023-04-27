using System;
using Windows.ApplicationModel.Resources;
using FileManager.Helpers;

namespace FileManager.ViewModels.Factory
{
    public class FileControlCreator
    {
        public static FileControlViewModel CreateFileControl(ResourceLoader themeResourceLoader, string name, string type, string path = "")
        {
            FileControlViewModel fileControl = new FileControlViewModel
            {
                DisplayName = name,
                Path = path,
            };
            if (type.Contains(Constants.Image, StringComparison.Ordinal))
            {
                fileControl.Image = themeResourceLoader.GetString(Constants.Image);
                fileControl.Type = Constants.Image;
            }
            else if (type.Contains(Constants.Video, StringComparison.Ordinal))
            {
                fileControl.Image = themeResourceLoader.GetString(Constants.Video);
                fileControl.Type = Constants.Video;
            }
            else if (type.Contains(Constants.Audio, StringComparison.Ordinal))
            {
                fileControl.Image = themeResourceLoader.GetString(Constants.Audio);
                fileControl.Type = Constants.Image;
            }
            else
            {
                fileControl.Image = themeResourceLoader.GetString(Constants.File);
                fileControl.Type = Constants.File;
            }

            return fileControl;
        }
    }
}
