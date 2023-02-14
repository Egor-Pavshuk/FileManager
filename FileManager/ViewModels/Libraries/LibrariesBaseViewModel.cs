using FileManager.Commands;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.ApplicationModel.Core;
using Windows.ApplicationModel.Resources;
using Windows.Storage;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

namespace FileManager.ViewModels.Libraries
{
    public abstract class LibrariesBaseViewModel : INotifyPropertyChanged
    {
        private bool isBackButtonAvailable;
        private bool isDeleteButtonAvailable;
        private bool isNewFolderButtonAvailable;
        private Color backgroundColor;
        private FileControlViewModel selectedGridItem;
        private IReadOnlyList<IStorageItem> storageItems;
        private Collection<FileControlViewModel> storageFiles;
        private string currentPath;
        private DoubleTappedEventHandler doubleClicked;
        private ItemClickEventHandler itemClicked;
        private ICommand getParentCommand;
        protected StorageFolder defaultFolder;
        protected StorageFolder currentFolder;
        protected ResourceLoader resourceLoader;
        protected UISettings settings;
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
        protected LibrariesBaseViewModel()
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

            settings = new UISettings();
            backgroundColor = settings.GetColorValue(UIColorType.Background);
            settings.ColorValuesChanged += ChangeColorMode;
            ChangeColorMode(settings, this);

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
                currentFolder = defaultFolder;

                await GetItemsAsync().ConfigureAwait(true);
                return;
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
            if (!(gridItems.SelectedItem is FileControlViewModel selectedItem) || selectedItem.Type == "File")
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
            if (!(gridItems.SelectedItem is FileControlViewModel selectedItem) || selectedItem.Type == "File")
            {
                return;
            }

            CurrentPath = selectedItem.Path;
            var newCurrentFolder = await StorageFolder.GetFolderFromPathAsync(CurrentPath);
            currentFolder = newCurrentFolder;

            StorageItems = await newCurrentFolder.GetFoldersAsync();
            await GetItemsAsync().ConfigureAwait(true);
        }

        private void ChangeColorMode(UISettings settings, object sender)
        {
            var currentBackgroundColor = settings.GetColorValue(UIColorType.Background);
            if (backgroundColor == currentBackgroundColor && storageFiles != null)
            {
                return;
            }

            if (currentBackgroundColor == Colors.Black)
            {
                resourceLoader = ResourceLoader.GetForViewIndependentUse("ImagesDark");
                backgroundColor = Colors.Black;
            }
            else
            {
                resourceLoader = ResourceLoader.GetForViewIndependentUse("ImagesLight");
                backgroundColor = Colors.White;
            }

            if (storageFiles is null)
            {
                return;
            }
            CoreApplication.MainView.CoreWindow.Dispatcher
                .RunAsync(CoreDispatcherPriority.Normal,
                () =>
                {
                    foreach (var file in storageFiles)
                    {
                        if (file.Type == "File")
                        {
                            file.Image = resourceLoader.GetString("File");
                        }
                        else
                        {
                            file.Image = resourceLoader.GetString("Folder");
                        }
                    }
                }).AsTask().ConfigureAwait(true);
        }
        protected abstract Task GetItemsAsync();
    }
}
