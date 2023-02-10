using FileManager.Commands;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

namespace FileManager.ViewModels.Libraries
{
    public class MusicsLibraryViewModel : LibrariesBaseViewModel
    {
        public MusicsLibraryViewModel()
        {
            currentFolder = KnownFolders.MusicLibrary;
            CurrentPath = currentFolder.Path;
            IsBackButtonAvailable = false;
            IsDeleteButtonAvailable = false;
            IsNewFolderButtonAvailable = false;
            _ = GetItemsAsync();
        }

        protected override async Task GetItemsAsync()
        {
            if (currentFolder.IsEqual(KnownFolders.MusicLibrary))
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
