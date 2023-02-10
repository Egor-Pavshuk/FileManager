using FileManager.Commands;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.Storage;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

namespace FileManager.ViewModels.Libraries
{
    public class LibrariesBaseViewModel : INotifyPropertyChanged
    {
        private bool isBackButtonAvailable;
        private bool isDeleteButtonAvailable;
        private bool isNewFolderButtonAvailable;
        private FileControlViewModel selectedGridItem;
        private IReadOnlyList<IStorageItem> storageItems;
        private Collection<FileControlViewModel> storageFiles;
        private string currentPath;
        private DoubleTappedEventHandler doubleClicked;
        private ItemClickEventHandler itemClicked;
        private ICommand getParentCommand;
        protected StorageFolder currentFolder;
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string prop = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }
        public bool IsBackButtonAvailable
        {
            get => isBackButtonAvailable;
            set
            {
                if (isBackButtonAvailable != value)
                {
                    isBackButtonAvailable = value;
                    OnPropertyChanged();
                }
            }
        }
        public bool IsDeleteButtonAvailable
        {
            get => isDeleteButtonAvailable;
            set
            {
                if (isDeleteButtonAvailable != value)
                {
                    isDeleteButtonAvailable = value;
                    OnPropertyChanged();
                }
            }
        }
        public bool IsNewFolderButtonAvailable
        {
            get => isNewFolderButtonAvailable;
            set
            {
                if (isNewFolderButtonAvailable != value)
                {
                    isNewFolderButtonAvailable = value;
                    OnPropertyChanged();
                }
            }
        }
        public FileControlViewModel SelectedItem
        {
            get => selectedGridItem;
            set
            {
                if (selectedGridItem != value)
                {
                    selectedGridItem = value;
                    OnPropertyChanged();
                }
            }
        }
        public IReadOnlyList<IStorageItem> StorageItems
        {
            get => storageItems;
            set
            {
                if (storageItems != value)
                {
                    storageItems = value;
                    OnPropertyChanged();
                }
            }
        }
        public Collection<FileControlViewModel> StorageFiles
        {
            get => storageFiles;
            set
            {
                if (storageFiles != value)
                {
                    storageFiles = value;
                    OnPropertyChanged();
                }
            }
        }
        public string CurrentPath
        {
            get => currentPath;
            set
            {
                if (currentPath != value)
                {
                    currentPath = value;
                    OnPropertyChanged();
                }
            }
        }
        public DoubleTappedEventHandler DoubleClicked
        {
            get => doubleClicked;
            set
            {
                if (doubleClicked != value)
                {
                    doubleClicked = value;
                    OnPropertyChanged();
                }
            }
        }
        public ItemClickEventHandler ItemClicked
        {
            get => itemClicked;
            set
            {
                if (itemClicked != value)
                {
                    itemClicked = value;
                    OnPropertyChanged();
                }
            }
        }
        public ICommand GetParentCommand
        {
            get => getParentCommand;
            set
            {
                if (getParentCommand != value)
                {
                    getParentCommand = value;
                    OnPropertyChanged();
                }
            }
        }
        public LibrariesBaseViewModel()
        {
            if (Windows.System.Profile.AnalyticsInfo.VersionInfo.DeviceFamily == "Windows.Xbox")
            {
                ItemClicked = OpenFolderXbox;
                DoubleClicked = (o, e) => { };
            }
            else
            {
                ItemClicked = (o, e) => { };
                DoubleClicked = OpenFolderWindows;
            }
            GetParentCommand = new RelayCommand(GetParent);
        }

        private async void GetParent(object sender)
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
            await GetItemsAsync().ConfigureAwait(true);
        }
        private async void OpenFolderXbox(object sender, ItemClickEventArgs e)
        {            
            if (sender is null)
            {
                return;
            }
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
            await GetItemsAsync().ConfigureAwait(true);
        }
        private async void OpenFolderWindows(object sender, DoubleTappedRoutedEventArgs e)
        {
            if (sender is null)
            {
                return;
            }
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
            await GetItemsAsync().ConfigureAwait(true);
        }

        protected virtual async Task GetItemsAsync()
        {
            if (currentFolder.IsEqual(KnownFolders.PicturesLibrary))
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
