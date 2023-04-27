using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;
using FileManager.ViewModels;
using FileManager.ViewModels.OnlineFileControls;

namespace FileManager.ViewModels.Factory
{
    public class FileControlCreator
    {
        public FileControlViewModel CreateFileControl()
        {
            return new FileControlViewModel();
        }
    }
}
