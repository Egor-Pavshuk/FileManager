using Windows.ApplicationModel.Resources;
using FileManager.Helpers;

namespace FileManager.ViewModels.OnlineFileControls
{
    public class ImageFileControlViewModel : OnlineFileControlViewModel
    {
        public ImageFileControlViewModel(ResourceLoader themeResourceLoader)
        {
            if (themeResourceLoader != null)
            {
                Image = themeResourceLoader.GetString(Constants.Image);
            }
            Type = Constants.Image;
        }
        public override void ChangeColorMode(ResourceLoader themeResourceLoader)
        {
            if (themeResourceLoader != null)
            {
                Image = themeResourceLoader.GetString(Constants.Image);
            }
        }
    }
}
