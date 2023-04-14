using Windows.Storage;

namespace FileManager.ViewModels.Libraries
{
    public class PicturesLibraryViewModel : LibrariesBaseViewModel
    {
        public PicturesLibraryViewModel() : base(KnownFolders.PicturesLibrary)
        { }
    }
}
