using Windows.ApplicationModel.Resources;
using FileManager.Helpers;

namespace FileManager.ViewModels.OnlineFileControls
{
    public class VideoFileControlViewModel : OnlineFileControlViewModel
    {
        public VideoFileControlViewModel(ResourceLoader themeResourceLoader)
        {
            if (themeResourceLoader != null)
            {
                Image = themeResourceLoader.GetString(Constants.Video);
            }
            Type = Constants.Video;
        }
        public override void ChangeColorMode(ResourceLoader themeResourceLoader)
        {
            if (themeResourceLoader != null)
            {
                Image = themeResourceLoader.GetString(Constants.Video);
            }
        }
    }
}
