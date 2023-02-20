namespace FileManager.ViewModels
{
    public class MainPageTitleViewModel : BindableBase
    {
        private string title;
        public string Title
        {
            get => title;
            set
            {
                if (title != value)
                {
                    title = value;
                    OnPropertyChanged();
                }
            }
        }
        public MainPageTitleViewModel()
        {
            Title = "Welcome to file manager!";
        }
    }
}
