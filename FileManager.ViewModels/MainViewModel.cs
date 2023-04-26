using System;
using Windows.ApplicationModel.Resources;
using Windows.UI.Xaml.Controls;
using Autofac;
using FileManager.Helpers;
using FileManager.Dependencies;

namespace FileManager.ViewModels
{
    public class MainViewModel : BindableBase
    {
        private const string GoogleDriveIconPath = "ms-appx:///Images/googleDrive.png";
        private const string FtpIconPath = "ms-appx:///Images/ftpFolder.png";
        private readonly ResourceLoader resourceLoader;
        private Page currentContent;
        private Page googleDrivePage;
        private string currentTitle;
        private NavigationViewItem selectedItem;
        private Uri googleDriveIconUri;
        private Uri ftpIconUri;

        public Page CurrentContent
        {
            get => currentContent;
            set
            {
                if (currentContent != value)
                {
                    currentContent = value;
                    OnPropertyChanged();
                }
            }
        }
        public NavigationViewItem SelectedItem
        {
            get => selectedItem;
            set
            {
                if (selectedItem != value)
                {
                    selectedItem = value;
                    SelectionChanged();
                    OnPropertyChanged();
                }
            }
        }
        public string CurrentTitle
        {
            get => currentTitle;
            set
            {
                if (currentTitle != value)
                {
                    currentTitle = value;
                    OnPropertyChanged();
                }
            }
        }
        public Uri GoogleDriveIconUri
        {
            get => googleDriveIconUri;
            set
            {
                if (googleDriveIconUri != value)
                {
                    googleDriveIconUri = value;
                    OnPropertyChanged();
                }
            }
        }
        public Uri FtpIconUri
        {
            get => ftpIconUri;
            set
            {
                if (ftpIconUri != value)
                {
                    ftpIconUri = value;
                    OnPropertyChanged();
                }
            }
        }

        public MainViewModel()
        {
            currentContent = (Page)VMDependencies.Container.Resolve(VMDependencies.Views[Constants.MainTitlePage]);
            resourceLoader = ResourceLoader.GetForCurrentView(Constants.StringResources);
            CurrentTitle = resourceLoader.GetString(Constants.MainPage);
            GoogleDriveIconUri = new Uri(GoogleDriveIconPath);
            FtpIconUri = new Uri(FtpIconPath);
        }

        private void SelectionChanged()
        {
            switch (selectedItem.AccessKey)
            {
                case "0":
                    CurrentContent = (Page)VMDependencies.Container.Resolve(VMDependencies.Views[Constants.PicturesLibraryPage]);
                    CurrentTitle = resourceLoader.GetString(Constants.ImageNavigation);
                    break;
                case "1":
                    CurrentContent = (Page)VMDependencies.Container.Resolve(VMDependencies.Views[Constants.VideosLibraryPage]);
                    CurrentTitle = resourceLoader.GetString(Constants.VideoNavigation);
                    break;
                case "2":
                    CurrentContent = (Page)VMDependencies.Container.Resolve(VMDependencies.Views[Constants.MusicsLibraryPage]);
                    CurrentTitle = resourceLoader.GetString(Constants.MusicNavigation);
                    break;
                case "3":
                    CurrentContent = (Page)VMDependencies.Container.Resolve(VMDependencies.Views[Constants.InformationPage]);
                    CurrentTitle = resourceLoader.GetString(Constants.InfoNavigation);
                    break;
                case "4":
                    if (googleDrivePage != null)
                    {
                        var dataContext = (GoogleDriveViewModel)googleDrivePage.DataContext;
                        if (!dataContext.IsErrorVisible)
                        {
                            CurrentContent = googleDrivePage;
                        }
                        else
                        {
                            CurrentContent = (Page )VMDependencies.Container.Resolve(VMDependencies.Views[Constants.GoogleDrivePage]);
                        }
                    }
                    else
                    {
                        CurrentContent = (Page)VMDependencies.Container.Resolve(VMDependencies.Views[Constants.GoogleDrivePage]);
                    }
                    googleDrivePage = currentContent;
                    CurrentTitle = resourceLoader.GetString(Constants.GoogleDriveNav);
                    break;
                case "5":
                    CurrentContent = (Page)VMDependencies.Container.Resolve(VMDependencies.Views[Constants.FtpPage]);
                    CurrentTitle = resourceLoader.GetString(Constants.FtpServer);
                    break;
                case "6":
                    CurrentContent = (Page)VMDependencies.Container.Resolve(VMDependencies.Views[Constants.OneDrivePage]);
                    CurrentTitle = resourceLoader.GetString(Constants.OneDrivePage);
                    break;
                default:
                    CurrentContent = (Page)VMDependencies.Container.Resolve(VMDependencies.Views[Constants.MainTitlePage]);
                    CurrentTitle = resourceLoader.GetString(Constants.MainNavigation);
                    break;
            }
        }
    }
}
