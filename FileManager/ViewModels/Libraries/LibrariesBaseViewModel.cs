using FileManager.Commands;
using FileManager.Validation;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.ApplicationModel.Core;
using Windows.ApplicationModel.Resources;
using Windows.Storage;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Popups;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

namespace FileManager.ViewModels.Libraries
{
    public class LibrariesBaseViewModel : BindableBase
    {
        private bool isBackButtonAvailable;
        private bool isDeleteButtonAvailable;
        private bool isNewFolderButtonAvailable;
        private FileControlViewModel editableItem;
        private FileControlViewModel selectedGridItem;
        private IReadOnlyList<IStorageItem> storageItems;
        private Collection<FileControlViewModel> storageFiles;
        private string currentPath;
        private DoubleTappedEventHandler doubleClicked;
        private ItemClickEventHandler itemClicked;
        private SelectionChangedEventHandler selectionChanged;
        private ICommand getParentCommand;
        private ICommand removeFileCommand;
        private ICommand createFolderCommand;
        private ICommand editSaveCommand;
        private StorageFolder defaultFolder;
        private StorageFolder currentFolder;
        private ResourceLoader themeResourceLoader;
        private ResourceLoader stringsResourceLoader;

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
        public FileControlViewModel SelectedGridItem
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
        public SelectionChangedEventHandler SelectionChanged
        {
            get => selectionChanged;
            set
            {
                if (selectionChanged != value)
                {
                    selectionChanged = value;
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
        public ICommand RemoveFileCommand
        {
            get => removeFileCommand;
            set
            {
                if (removeFileCommand != value)
                {
                    removeFileCommand = value;
                    OnPropertyChanged();
                }
            }
        }
        public ICommand CreateFolderCommand
        {
            get => createFolderCommand;
            set
            {
                if (createFolderCommand != value)
                {
                    createFolderCommand = value;
                    OnPropertyChanged();
                }
            }
        }
        public ICommand EditSaveCommand
        {
            get => editSaveCommand;
            set
            {
                if (editSaveCommand != value)
                {
                    editSaveCommand = value;
                    OnPropertyChanged();
                }
            }
        }

        public LibrariesBaseViewModel(string libraryName)
        {
            const string resources = "Resources";
            if (Windows.System.Profile.AnalyticsInfo.VersionInfo.DeviceFamily == "Windows.Xbox")
            {
                ItemClicked = OpenFolderXbox;
                DoubleClicked = (o, e) => { };
            }
            else
            {
                DoubleClicked = OpenFolderWindows;
                ItemClicked = (o, e) => { };
            }

            switch (libraryName)
            {
                case "Pictures":
                    defaultFolder = KnownFolders.PicturesLibrary;
                    break;
                case "Videos":
                    defaultFolder = KnownFolders.VideosLibrary;
                    break;
                case "Music":
                    defaultFolder = KnownFolders.MusicLibrary;
                    break;
                default:
                    break;
            }            
            currentFolder = defaultFolder;
            GetItemsAsync().ConfigureAwait(true);

            ChangeColorMode(settings, this);
            SelectionChanged = GridSelectionChanged;

            SelectedGridItem = new FileControlViewModel();
            stringsResourceLoader = ResourceLoader.GetForCurrentView(resources);

            GetParentCommand = new RelayCommand(GetParentAsync);
            RemoveFileCommand = new RelayCommand(RemoveFileAsync);
            CreateFolderCommand = new RelayCommand(CreateFolder);
            EditSaveCommand = new RelayCommand(RenameItem);
        }

        protected override void ChangeColorMode(UISettings uiSettings, object sender)
        {
            const string folder = "folder";
            const string file = "file";
            const string imagesDark = "ImagesDark";
            const string imagesLight = "ImagesLight";

            var currentBackgroundColor = uiSettings?.GetColorValue(UIColorType.Background);
            if (backgroundColor != currentBackgroundColor || storageFiles == null)
            {
                if (currentBackgroundColor == Colors.Black)
                {
                    themeResourceLoader = ResourceLoader.GetForViewIndependentUse(imagesDark);
                    backgroundColor = Colors.Black;
                }
                else
                {
                    themeResourceLoader = ResourceLoader.GetForViewIndependentUse(imagesLight);
                    backgroundColor = Colors.White;
                }

                if (storageFiles != null)
                {                  
                    CoreApplication.MainView.CoreWindow.Dispatcher
                    .RunAsync(CoreDispatcherPriority.Normal,
                    () =>
                    {
                        foreach (var storageFile in storageFiles)
                        {
                            if (storageFile.Type == "File")
                            {
                                storageFile.Image = themeResourceLoader.GetString(file);
                            }
                            else
                            {
                                storageFile.Image = themeResourceLoader.GetString(folder);
                            }
                        }
                    }).AsTask().ConfigureAwait(true);
                }
            }
        }

        private async Task GetItemsAsync()
        {
            const string folder = "folder";
            const string file = "file";

            if (currentFolder.IsEqual(KnownFolders.PicturesLibrary) || currentFolder.IsEqual(KnownFolders.VideosLibrary) || currentFolder.IsEqual(KnownFolders.MusicLibrary))
            {
                StorageItems = await currentFolder.GetItemsAsync();
            }

            Collection<FileControlViewModel> fileControls = new Collection<FileControlViewModel>();
            foreach (var item in StorageItems)
            {
                var fileControl = new FileControlViewModel() { Image = themeResourceLoader.GetString(folder), DisplayName = item.Name, Path = item.Path, Type = "Folder" };
                fileControls.Add(fileControl);
            }

            IReadOnlyList<StorageFile> storageFiles = await currentFolder.GetFilesAsync();
            foreach (var item in storageFiles)
            {
                var viewModel = new FileControlViewModel() { Image = themeResourceLoader.GetString(file), DisplayName = item.Name, Path = item.Path, Type = "File" };
                fileControls.Add(viewModel);
            }
            StorageFiles = fileControls;
        }        

        private void GridSelectionChanged(object selder, SelectionChangedEventArgs e)
        {
            if (editableItem != null)
            {
                var file = StorageFiles.First(f => f.Path == editableItem.Path);
                file.DisplayName = editableItem.DisplayName;
                file.IsEditMode = false;
                file.IsReadOnlyMode = true;
                EditSaveCommand = new RelayCommand(RenameItem);

                editableItem = null;
            }
        }

        private void RenameItem(object sender)
        {
            if (selectedGridItem != null && editableItem is null)
            {
                editableItem = new FileControlViewModel()
                {
                    DisplayName = selectedGridItem.DisplayName,
                    Type = selectedGridItem.Type,
                    Image = selectedGridItem.Image,
                    Path = selectedGridItem.Path,
                };

                selectedGridItem.IsEditMode = true;
                selectedGridItem.IsReadOnlyMode = false;

                EditSaveCommand = new RelayCommand(SaveChangesAsync);
            }
        }

        private async void SaveChangesAsync(object sender)
        {
            const string sameNameError = "sameNameError";
            const string inputError = "inputError";
            const string invalidInput = "invalidInput";
            const string confirmation = "confirmation";
            const string renameConfirmText = "renameConfirmText";
            const string yesButton = "yesButton";
            const string cancelButton = "cancelButton";
            bool saveChanges = false;

            if (selectedGridItem != null && selectedGridItem.IsEditMode)
            {
                if (selectedGridItem.DisplayName.EndsWith(' '))
                {
                    SelectedGridItem.DisplayName = selectedGridItem.DisplayName.Remove(selectedGridItem.DisplayName.Length - 1);
                }

                if (editableItem.DisplayName != selectedGridItem.DisplayName)
                {
                    if (StorageFiles.Count(f => f.DisplayName == selectedGridItem.DisplayName) > 1)
                    {
                        var messageDialog = new MessageDialog(stringsResourceLoader.GetString(sameNameError))
                        {
                            Title = stringsResourceLoader.GetString(inputError)
                        };
                        await messageDialog.ShowAsync();
                    }
                    else if (!ItemNameValidation.Validate(selectedGridItem.DisplayName))
                    {
                        var messageDialog = new MessageDialog(stringsResourceLoader.GetString(invalidInput))
                        {
                            Title = stringsResourceLoader.GetString(inputError)
                        };
                        await messageDialog.ShowAsync();
                    }
                    else
                    {
                        var contentDialog = new ContentDialog()
                        {
                            Title = stringsResourceLoader.GetString(confirmation),
                            Content = stringsResourceLoader.GetString(renameConfirmText),
                            PrimaryButtonText = stringsResourceLoader.GetString(yesButton),
                            CloseButtonText = stringsResourceLoader.GetString(cancelButton),
                        };

                        var confirmationResult = await contentDialog.ShowAsync();
                        if (confirmationResult == ContentDialogResult.Primary)
                        {
                            IStorageItem item = await currentFolder.GetItemAsync(editableItem.DisplayName);
                            await item.RenameAsync(selectedGridItem.DisplayName);
                            selectedGridItem.Path = currentPath + "\\" + selectedGridItem.DisplayName;
                        }
                        else
                        {
                            SelectedGridItem.DisplayName = editableItem.DisplayName;
                        }     
                        
                        saveChanges = true;
                    }                    
                }
                else
                {
                    saveChanges = true;
                }                
            }
            else
            {
                saveChanges = true;
            }

            if (saveChanges)
            {
                EditSaveCommand = new RelayCommand(RenameItem);
                SelectedGridItem.IsEditMode = false;
                SelectedGridItem.IsReadOnlyMode = true;
                editableItem = null;
            }
        }

        private async void GetParentAsync(object sender)
        {
            if (editableItem != null)
            {
                GridSelectionChanged(sender, null);
            }

            var newCurrentFolder = await currentFolder.GetParentAsync();
            if (await newCurrentFolder.GetParentAsync() != null)
            {
                currentFolder = newCurrentFolder;
                CurrentPath = newCurrentFolder.Path;

                StorageItems = await newCurrentFolder.GetFoldersAsync();
                await GetItemsAsync().ConfigureAwait(true);
            }
            else
            {
                IsBackButtonAvailable = false;
                IsDeleteButtonAvailable = false;
                IsNewFolderButtonAvailable = false;
                currentFolder = defaultFolder;

                await GetItemsAsync().ConfigureAwait(true);
            }            
        }

        private async void RemoveFileAsync(object sender)
        {
            const string confirmation = "confirmation";
            const string deleteConfirmText = "deleteConfirmText";
            const string yesButton = "yesButton";
            const string cancelButton = "cancelButton";

            if (SelectedGridItem != null && SelectedGridItem.IsReadOnlyMode)
            {               
                var contentDialog = new ContentDialog()
                {
                    Title = stringsResourceLoader.GetString(confirmation),
                    Content = stringsResourceLoader.GetString(deleteConfirmText) + $" \"{selectedGridItem.DisplayName}\"?",
                    PrimaryButtonText = stringsResourceLoader.GetString(yesButton),
                    CloseButtonText = stringsResourceLoader.GetString(cancelButton),
                };

                var confirmationResult = await contentDialog.ShowAsync();
                if (confirmationResult == ContentDialogResult.Primary)
                {
                    string itemName = selectedGridItem.DisplayName;
                    Collection<FileControlViewModel> files = new Collection<FileControlViewModel>();

                    StorageFiles.Remove(SelectedGridItem);
                    foreach (var file in StorageFiles)
                    {
                        files.Add(file);
                    }
                    StorageFiles = files;
                    IStorageItem item = await currentFolder.GetItemAsync(itemName);
                    await item.DeleteAsync();

                    SelectedGridItem = null;
                }
            }            
        }

        private void CreateFolder(object sender)
        {
            const string folder = "folder";

            int countOfNewFolders = StorageFiles.Where(f => f.Type == "Folder" && f.DisplayName.StartsWith("New folder")).Count();
            currentFolder.CreateFolderAsync($"New folder {countOfNewFolders + 1}").Completed +=
                async (i, s) =>
            await CoreApplication.MainView.CoreWindow.Dispatcher
                .RunAsync(CoreDispatcherPriority.Normal,
                () =>
                {
                    List<FileControlViewModel> files = new List<FileControlViewModel>();
                    foreach (var file in StorageFiles)
                    {
                        files.Add(file);
                    }

                    var newFolder = new FileControlViewModel()
                    {
                        Image = themeResourceLoader.GetString(folder),
                        DisplayName = $"New folder {countOfNewFolders + 1}",
                        Type = "Folder",
                        Path = currentFolder.Path + $"\\New folder {countOfNewFolders + 1}"
                    };

                    files.Insert(files.FindLastIndex(f => f.Type == "Folder") + 1, newFolder);
                    StorageFiles = new Collection<FileControlViewModel>(files);
                    SelectedGridItem = newFolder;
                });
        }

        private async void OpenFolderXbox(object sender, ItemClickEventArgs e)
        {
            if (sender != null)
            {
                var gridItems = (GridView)sender;
                if (gridItems.SelectedItem is FileControlViewModel selectedItem && selectedItem.Type != "File" && !string.IsNullOrEmpty(selectedItem.DisplayName))
                {
                    IsBackButtonAvailable = true;
                    IsDeleteButtonAvailable = true;
                    IsNewFolderButtonAvailable = true;

                    CurrentPath = selectedItem.Path;
                    var newCurrentFolder = await StorageFolder.GetFolderFromPathAsync(CurrentPath);
                    currentFolder = newCurrentFolder;

                    StorageItems = await newCurrentFolder.GetFoldersAsync();
                    await GetItemsAsync().ConfigureAwait(true);
                }
            }
        }

        private async void OpenFolderWindows(object sender, DoubleTappedRoutedEventArgs e)
        {
            if (sender != null)
            {
                var gridItems = (GridView)sender;
                if (gridItems.SelectedItem is FileControlViewModel selectedItem && selectedItem.Type != "File" && !string.IsNullOrEmpty(selectedItem.DisplayName))
                {
                    IsBackButtonAvailable = true;
                    IsDeleteButtonAvailable = true;
                    IsNewFolderButtonAvailable = true;

                    CurrentPath = selectedItem.Path;
                    var newCurrentFolder = await StorageFolder.GetFolderFromPathAsync(CurrentPath);
                    currentFolder = newCurrentFolder;

                    StorageItems = await newCurrentFolder.GetFoldersAsync();
                    await GetItemsAsync().ConfigureAwait(true);
                }                
            }             
        }
    }
}
