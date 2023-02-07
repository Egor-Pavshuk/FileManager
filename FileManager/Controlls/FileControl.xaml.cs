using FileManager.ViewModels;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace FileManager.Controlls
{
    public sealed partial class FileControl : UserControl
    {
        public static readonly DependencyProperty FileControlProperty =
        DependencyProperty.Register(
            "FileControlViewModel",
            typeof(FileControlViewModel),
            typeof(FileControl),
            new PropertyMetadata(null));
        public FileControl()
        {
            this.InitializeComponent();
        }
    }
}
