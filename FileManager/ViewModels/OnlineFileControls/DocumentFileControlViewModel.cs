using FileManager.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources;

namespace FileManager.ViewModels.OnlineFileControls
{
    public class DocumentFileControlViewModel : OnlineFileControlViewModel
    {
        public DocumentFileControlViewModel(ResourceLoader themeResourceLoader)
        {
            if (themeResourceLoader != null)
            {
                Image = themeResourceLoader.GetString(Constants.File);
            }
            Type = Constants.File;
        }
        public override void ChangeColorMode(ResourceLoader themeResourceLoader)
        {
            if (themeResourceLoader != null)
            {
                Image = themeResourceLoader.GetString(Constants.File);
            }
        }
    }
}
