﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Windows.Storage;

namespace FileManager.ViewModels.Libraries
{
    public class MusicsLibraryViewModel : LibrariesBaseViewModel
    {
        public MusicsLibraryViewModel()
        {
            defaultFolder = KnownFolders.MusicLibrary;
            currentFolder = defaultFolder;
            CurrentPath = currentFolder.Path;

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
                var fileControl = new FileControlViewModel() { Image = resourceLoader.GetString("folder"), DisplayName = item.Name, Path = item.Path, Type = "Folder" };
                fileControls.Add(fileControl);
            }

            IReadOnlyList<StorageFile> storageFiles = await currentFolder.GetFilesAsync();
            foreach (var item in storageFiles)
            {
                var viewModel = new FileControlViewModel() { Image = resourceLoader.GetString("file"), DisplayName = item.Name, Path = item.Path, Type = "File" };
                fileControls.Add(viewModel);
            }
            StorageFiles = fileControls;
        }
    }
}
