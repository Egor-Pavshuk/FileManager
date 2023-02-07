using FileManager.ViewModels;
using Windows.UI.Xaml.Controls;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace FileManager.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class DirectoryFilesPage : Page
    {
        public DirectoryFilesViewModel ViewModel { get; set; } = new DirectoryFilesViewModel();
        public DirectoryFilesPage()
        {
            this.InitializeComponent();
            this.DataContext = ViewModel;
        }
    }
}
