using Windows.Storage;

namespace FileManager.ViewModels.Libraries
{
    public class MusicsLibraryViewModel : LibrariesBaseViewModel
    {
        public MusicsLibraryViewModel() : base(KnownFolders.MusicLibrary)
        { }
    }
}
