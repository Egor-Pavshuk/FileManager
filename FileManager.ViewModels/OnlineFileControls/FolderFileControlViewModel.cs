using Windows.ApplicationModel.Resources;
using FileManager.Helpers;

namespace FileManager.ViewModels.OnlineFileControls
{
    public class FolderFileControlViewModel : OnlineFileControlViewModel
    {
        public FolderFileControlViewModel(ResourceLoader themeResourceLoader)
        {
            if (themeResourceLoader != null)
            {
                Image = themeResourceLoader.GetString(Constants.Folder);
            }
            Type = Constants.Folder;
        }
        public override void ChangeColorMode(ResourceLoader themeResourceLoader)
        {
            if (themeResourceLoader != null)
            {
                Image = themeResourceLoader.GetString(Constants.Folder);
            }
        }
    }
}
