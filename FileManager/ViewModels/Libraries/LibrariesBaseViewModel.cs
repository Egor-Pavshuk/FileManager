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
using Windows.System;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Popups;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml.Controls;

namespace FileManager.ViewModels.Libraries
{
    public class LibrariesBaseViewModel : BindableBase
    {
        private bool isBackButtonAvailable;
        private bool isDeleteButtonAvailable;
        private bool isNewFolderButtonAvailable;
        private bool isSaveButtonVisible;
        private FileControlViewModel editableItem;
        private FileControlViewModel selectedGridItem;
        private IReadOnlyList<IStorageItem> storageItems;
        private Collection<FileControlViewModel> storageFiles;
        private string currentPath;
        private ICommand doubleClicked;
        private ICommand itemClicked;
        private ICommand selectionChanged;
        private ICommand getParentCommand;
        private ICommand removeFileCommand;
        private ICommand createFolderCommand;
        private ICommand editSaveCommand;
        private readonly StorageFolder defaultFolder;
        private StorageFolder currentFolder;
        private ResourceLoader themeResourceLoader;
        private readonly ResourceLoader stringsResourceLoader;

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
        public bool IsSaveButtonVisible
        {
            get => isSaveButtonVisible;
            set
            {
                if (isSaveButtonVisible != value)
                {
                    isSaveButtonVisible = value;
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
        public ICommand DoubleClickedCommand
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
        public ICommand ItemClickedCommand
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
        public ICommand SelectionChangedCommand
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
                ItemClickedCommand = new RelayCommand(OpenFileXboxAsync);
                DoubleClickedCommand = new RelayCommand((o) => { });
            }
            else
            {
                DoubleClickedCommand = new RelayCommand(OpenFileWindowsAsync);
                ItemClickedCommand = new RelayCommand((o) => { });
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
            SelectionChangedCommand = new RelayCommand(GridSelectionChanged);

            stringsResourceLoader = ResourceLoader.GetForCurrentView(resources);

            GetParentCommand = new RelayCommand(GetParentAsync);
            RemoveFileCommand = new RelayCommand(RemoveFileAsync);
            CreateFolderCommand = new RelayCommand(CreateFolder);
            EditSaveCommand = new RelayCommand(RenameItem);
        }

        protected override void ChangeColorMode(UISettings uiSettings, object sender)
        {
            const string image = "image";
            const string video = "video";
            const string audio = "audio";
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
                            switch (storageFile.Type)
                            {
                                case image:
                                    storageFile.Image = themeResourceLoader.GetString(image);
                                    break;
                                case video:
                                    storageFile.Image = themeResourceLoader.GetString(video);
                                    break;
                                case audio:
                                    storageFile.Image = themeResourceLoader.GetString(audio);
                                    break;
                                case folder:
                                    storageFile.Image = themeResourceLoader.GetString(folder);
                                    break;
                                default:
                                    storageFile.Image = themeResourceLoader.GetString(file);
                                    break;
                            }
                        }
                    }).AsTask().ConfigureAwait(true);
                }
            }
        }

        private async Task GetItemsAsync()
        {
            const string image = "image";
            const string video = "video";
            const string audio = "audio";
            const string folder = "folder";
            const string file = "file";

            if (currentFolder.IsEqual(KnownFolders.PicturesLibrary) || currentFolder.IsEqual(KnownFolders.VideosLibrary) || currentFolder.IsEqual(KnownFolders.MusicLibrary))
            {
                StorageItems = await currentFolder.GetItemsAsync();
            }
            else
            {
                StorageItems = await currentFolder.GetFoldersAsync();
            }

            Collection<FileControlViewModel> fileControls = new Collection<FileControlViewModel>();
            foreach (var item in StorageItems)
            {
                if (item.IsOfType(StorageItemTypes.Folder))
                {
                    var fileControl = new FileControlViewModel() { Image = themeResourceLoader.GetString(folder), DisplayName = item.Name, Path = item.Path, Type = folder };
                    fileControls.Add(fileControl);
                }
            }

            IReadOnlyList<StorageFile> storageFiles = await currentFolder.GetFilesAsync();
            foreach (var item in storageFiles)
            {
                FileControlViewModel viewModel;
                if (item.ContentType.Contains(image, StringComparison.Ordinal))
                {
                    viewModel = new FileControlViewModel() { Image = themeResourceLoader.GetString(image), DisplayName = item.Name, Path = item.Path, Type = image };
                }
                else if (item.ContentType.Contains(video, StringComparison.Ordinal))
                {
                    viewModel = new FileControlViewModel() { Image = themeResourceLoader.GetString(video), DisplayName = item.Name, Path = item.Path, Type = video };
                }
                else if (item.ContentType.Contains(audio, StringComparison.Ordinal))
                {
                    viewModel = new FileControlViewModel() { Image = themeResourceLoader.GetString(audio), DisplayName = item.Name, Path = item.Path, Type = audio };
                }
                else
                {
                    viewModel = new FileControlViewModel() { Image = themeResourceLoader.GetString(file), DisplayName = item.Name, Path = item.Path, Type = file };
                }

                fileControls.Add(viewModel);
            }
            StorageFiles = fileControls;
        }

        private void GridSelectionChanged(object selder)
        {
            IsSaveButtonVisible = false;
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

                IsSaveButtonVisible = true;
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
            const string warning = "warning";
            const string changeTypeConfirmText = "changeTypeConfirmText";
            const string folder = "folder";
            bool saveChanges = false;

            if (selectedGridItem != null && selectedGridItem.IsEditMode)
            {
                if (selectedGridItem.DisplayName.EndsWith(' '))
                {
                    SelectedGridItem.DisplayName = selectedGridItem.DisplayName.Remove(selectedGridItem.DisplayName.Length - 1);
                }

                if (editableItem.DisplayName != selectedGridItem.DisplayName)
                {
                    var editableItemDisplayName = editableItem.DisplayName;
                    var selectedItemDisplayName = selectedGridItem.DisplayName;

                    if (StorageFiles.Count(f => f.DisplayName == selectedItemDisplayName) > 1)
                    {
                        var messageDialog = new MessageDialog(stringsResourceLoader.GetString(sameNameError))
                        {
                            Title = stringsResourceLoader.GetString(inputError)
                        };
                        await messageDialog.ShowAsync();
                    }
                    else if (!ItemNameValidation.Validate(selectedItemDisplayName))
                    {
                        var messageDialog = new MessageDialog(stringsResourceLoader.GetString(invalidInput))
                        {
                            Title = stringsResourceLoader.GetString(inputError)
                        };
                        await messageDialog.ShowAsync();
                    }
                    else
                    {
                        var confirmationContentDialog = new ContentDialog()
                        {
                            Title = stringsResourceLoader.GetString(confirmation),
                            Content = stringsResourceLoader.GetString(renameConfirmText),
                            PrimaryButtonText = stringsResourceLoader.GetString(yesButton),
                            CloseButtonText = stringsResourceLoader.GetString(cancelButton),
                        };

                        var confirmationResult = await confirmationContentDialog.ShowAsync();

                        if (editableItem.Type != folder &&
                        editableItemDisplayName.Substring(editableItemDisplayName.LastIndexOf('.')) != selectedItemDisplayName.Substring(selectedItemDisplayName.LastIndexOf('.')) &&
                        confirmationResult == ContentDialogResult.Primary)
                        {
                            var changeTypeContentDialog = new ContentDialog()
                            {
                                Title = stringsResourceLoader.GetString(warning),
                                Content = stringsResourceLoader.GetString(changeTypeConfirmText),
                                PrimaryButtonText = stringsResourceLoader.GetString(yesButton),
                                CloseButtonText = stringsResourceLoader.GetString(cancelButton),
                            };

                            confirmationResult = await changeTypeContentDialog.ShowAsync();
                        }

                        if (confirmationResult == ContentDialogResult.Primary)
                        {
                            IStorageItem item = await currentFolder.GetItemAsync(editableItem.DisplayName);
                            await item.RenameAsync(selectedItemDisplayName);
                            selectedGridItem.Path = currentPath + "\\" + selectedItemDisplayName;
                        }
                        else
                        {
                            SelectedGridItem.DisplayName = editableItemDisplayName;
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
                IsSaveButtonVisible = false;
                editableItem = null;
            }
        }

        private async void GetParentAsync(object sender)
        {
            if (editableItem != null)
            {
                GridSelectionChanged(sender);
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
                SelectedGridItem = new FileControlViewModel();

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
            const string newFolderName = "New folder";

            int countOfNewFolders = StorageFiles.Where(f => f.Type == folder && f.DisplayName.StartsWith(newFolderName)).Count();
            currentFolder.CreateFolderAsync($"{newFolderName} {countOfNewFolders + 1}").Completed +=
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
                        DisplayName = $"{newFolderName} {countOfNewFolders + 1}",
                        Type = folder,
                        Path = currentFolder.Path + $"\\{newFolderName} {countOfNewFolders + 1}"
                    };

                    files.Insert(files.FindLastIndex(f => f.Type == folder) + 1, newFolder);
                    StorageFiles = new Collection<FileControlViewModel>(files);
                    SelectedGridItem = newFolder;
                });
        }

        private async void OpenFileXboxAsync(object sender)
        {
            const string folder = "folder";

            if (editableItem != null)
            {
                GridSelectionChanged(sender);
            }

            if (sender != null)
            {
                var gridItems = (GridView)sender;
                if (gridItems.SelectedItem is FileControlViewModel selectedItem && !string.IsNullOrEmpty(selectedItem.DisplayName))
                {
                    if (selectedItem.Type != folder)
                    {
                        var file = await currentFolder.GetFileAsync(selectedItem.DisplayName);

                        if (file != null)
                        {
                            await Launcher.LaunchFileAsync(file);
                        }
                    }
                    else
                    {
                        IsBackButtonAvailable = true;
                        IsDeleteButtonAvailable = true;
                        IsNewFolderButtonAvailable = true;

                        CurrentPath = selectedItem.Path;
                        var newCurrentFolder = await StorageFolder.GetFolderFromPathAsync(CurrentPath);
                        currentFolder = newCurrentFolder;

                        await GetItemsAsync().ConfigureAwait(true);
                    }
                }
            }
        }

        private async void OpenFileWindowsAsync(object sender)
        {
            const string folder = "folder";

            if (editableItem != null)
            {
                GridSelectionChanged(sender);
            }

            if (sender != null)
            {
                var gridItems = (GridView)sender;
                if (gridItems.SelectedItem is FileControlViewModel selectedItem && !string.IsNullOrEmpty(selectedItem.DisplayName))
                {
                    if (selectedItem.Type != folder)
                    {
                        var file = await currentFolder.GetFileAsync(selectedItem.DisplayName);

                        if (file != null)
                        {
                            await Launcher.LaunchFileAsync(file);
                        }
                    }
                    else
                    {
                        IsBackButtonAvailable = true;
                        IsDeleteButtonAvailable = true;
                        IsNewFolderButtonAvailable = true;

                        CurrentPath = selectedItem.Path;
                        var newCurrentFolder = await StorageFolder.GetFolderFromPathAsync(CurrentPath);
                        currentFolder = newCurrentFolder;

                        await GetItemsAsync().ConfigureAwait(true);
                    }
                }
            }
        }
    }
}
