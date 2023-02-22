using FileManager.ViewModels.Libraries;
using Windows.UI.Xaml.Controls;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace FileManager.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class PicturesLibraryPage : Page
    {
        private const string picturePage = "Pictures";
        public LibrariesBaseViewModel ViewModel { get; set; } = new LibrariesBaseViewModel(picturePage);
        public PicturesLibraryPage()
        {
            this.InitializeComponent();
            this.DataContext = ViewModel;
        }
    }
}
