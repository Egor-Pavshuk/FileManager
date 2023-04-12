using FileManager.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources;

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
