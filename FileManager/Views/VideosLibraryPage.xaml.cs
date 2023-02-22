using FileManager.ViewModels.Libraries;
using Windows.UI.Xaml.Controls;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace FileManager.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class VideosLibraryPage : Page
    {
        const string videoPage = "Videos";
        public LibrariesBaseViewModel ViewModel { get; set; } = new LibrariesBaseViewModel(videoPage);

        public VideosLibraryPage()
        {
            this.InitializeComponent();
            DataContext = ViewModel;
        }
    }
}
