using FileManager.ViewModels;
using Windows.UI.Xaml.Controls;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace FileManager.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPageTitle : Page
    {
        public MainPageTitleViewModel ViewModel { get; set; } = new MainPageTitleViewModel();
        public MainPageTitle()
        {
            this.InitializeComponent();
            DataContext = ViewModel;
        }
    }
}
