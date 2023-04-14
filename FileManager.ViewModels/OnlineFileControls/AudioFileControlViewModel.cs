using Windows.ApplicationModel.Resources;
using FileManager.Helpers;

namespace FileManager.ViewModels.OnlineFileControls
{
    public class AudioFileControlViewModel : OnlineFileControlViewModel
    {
        public AudioFileControlViewModel(ResourceLoader themeResourceLoader)
        {
            if (themeResourceLoader != null)
            {
                Image = themeResourceLoader.GetString(Constants.Audio);
            }
            Type = Constants.Audio;
        }
        public override void ChangeColorMode(ResourceLoader themeResourceLoader)
        {
            if (themeResourceLoader != null)
            {
                Image = themeResourceLoader.GetString(Constants.Audio);
            }
        }
    }
}
