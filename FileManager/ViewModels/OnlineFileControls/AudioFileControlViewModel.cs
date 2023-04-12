using FileManager.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources;

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
