using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.ViewManagement;

namespace FileManager.ViewModels
{
    public class BindableBase : INotifyPropertyChanged
    {
        protected Color backgroundColor;
        protected UISettings settings;
        public event PropertyChangedEventHandler PropertyChanged;

        public BindableBase()
        {
            settings = new UISettings();
            backgroundColor = settings.GetColorValue(UIColorType.Background);
            settings.ColorValuesChanged += ChangeColorMode;
        }
        public void OnPropertyChanged([CallerMemberName] string prop = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }
        protected virtual void ChangeColorMode(UISettings settings, object sender) { }
    }
}
