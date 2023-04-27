using Windows.ApplicationModel.Resources;
using Windows.UI;

namespace FileManager.Models
{
    public class ColorMode
    {
        public Color BackgroundColor { get; set; }
        public ResourceLoader ThemeResourceLoader { get; set; }
    }
}
