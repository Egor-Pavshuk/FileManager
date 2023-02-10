using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

namespace FileManager.ViewModels.Libraries
{
    public class VideosLibraryViewModel : LibrariesBaseViewModel
    {
        public VideosLibraryViewModel()
        {
            currentFolder = KnownFolders.VideosLibrary;
            CurrentPath = currentFolder.Path;
            IsBackButtonAvailable = false;
            IsDeleteButtonAvailable = false;
            IsNewFolderButtonAvailable = false;
            _ = GetItemsAsync();
        }
        protected override async Task GetItemsAsync()
        {
            if (currentFolder.IsEqual(KnownFolders.VideosLibrary))
            {
                StorageItems = await currentFolder.GetItemsAsync();
            }

            Collection<FileControlViewModel> fileControls = new Collection<FileControlViewModel>();
            foreach (var item in StorageItems)
            {
                var fileControl = new FileControlViewModel() { Image = "/Images/Folder.jpg", DisplayName = item.Name, Path = item.Path };
                fileControls.Add(fileControl);
            }

            IReadOnlyList<StorageFile> storageFiles = await currentFolder.GetFilesAsync();
            foreach (var item in storageFiles)
            {
                var viewModel = new FileControlViewModel() { Image = "/Images/File.png", DisplayName = item.Name, Path = item.Path };
                fileControls.Add(viewModel);
            }
            StorageFiles = fileControls;
        }
    }
}
