using Windows.Storage;

namespace FileManager.ViewModels.Libraries
{
    public class VideosLibraryViewModel : LibrariesBaseViewModel
    {
        public VideosLibraryViewModel() : base(KnownFolders.VideosLibrary)
        { }
    }
}
