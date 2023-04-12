using FileManager.Controlls;
using FileManager.ViewModels;
using FileManager.ViewModels.OnlineFileControls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;

namespace FileManager.Factory
{
    public class FileControlCreator
    {
        public FileControlViewModel CreateFileControl()
        {
            return new FileControlViewModel();
        }
    }
}
