using Windows.ApplicationModel.Resources;
using FileManager.Helpers;

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
