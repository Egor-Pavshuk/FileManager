using System.ComponentModel;
using System.Runtime.CompilerServices;
using Windows.UI;
using Windows.UI.ViewManagement;

namespace FileManager.ViewModels
{
    public class BindableBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected Color backgroundColor;
        protected UISettings settings;

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
        protected virtual void ChangeColorMode(UISettings settings, object sender)
        { }
    }
}
