using System;
using System.Collections.Generic;
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
        public override async void GetParent()
        {
            var newCurrentFolder = await currentFolder.GetParentAsync();
            if (await newCurrentFolder.GetParentAsync() is null)
            {
                IsBackButtonAvailable = false;
                IsDeleteButtonAvailable = false;
                IsNewFolderButtonAvailable = false;
            }
            currentFolder = newCurrentFolder;
            CurrentPath = newCurrentFolder.Path;

            StorageItems = await newCurrentFolder.GetFoldersAsync();
            await GetItemsAsync();
        }

        public override async void OpenFolder(object sender, DoubleTappedRoutedEventArgs e)
        {
            IsBackButtonAvailable = true;
            IsDeleteButtonAvailable = true;
            IsNewFolderButtonAvailable = true;
            var gridItems = sender as GridView;
            if (!(gridItems.SelectedItem is FileControlViewModel selectedItem))
            {
                return;
            }

            CurrentPath = selectedItem.Path;
            var newCurrentFolder = await StorageFolder.GetFolderFromPathAsync(CurrentPath);
            currentFolder = newCurrentFolder;

            StorageItems = await newCurrentFolder.GetFoldersAsync();
            await GetItemsAsync();
        }

        protected override async Task GetItemsAsync()
        {
            if (currentFolder.IsEqual(KnownFolders.MusicLibrary))
            {
                StorageItems = await currentFolder.GetItemsAsync();
            }

            List<FileControlViewModel> fileControls = new List<FileControlViewModel>();
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
