using System;
using Windows.ApplicationModel.Resources;
using Windows.UI.Xaml.Controls;
using FileManager.Helpers;
using FileManager.Views;

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
            currentContent = new MainTitlePage();
            resourceLoader = ResourceLoader.GetForCurrentView(Constants.Resourses);
            CurrentTitle = resourceLoader.GetString(Constants.MainPage);
            GoogleDriveIconUri = new Uri(GoogleDriveIconPath);
            FtpIconUri = new Uri(FtpIconPath);
        }

        private void SelectionChanged()
        {
            switch (selectedItem.AccessKey)
            {
                case "0":
                    CurrentContent = new PicturesLibraryPage();
                    CurrentTitle = resourceLoader.GetString(Constants.ImageNavigation);
                    break;
                case "1":
                    CurrentContent = new VideosLibraryPage();
                    CurrentTitle = resourceLoader.GetString(Constants.VideoNavigation);
                    break;
                case "2":
                    CurrentContent = new MusicsLibraryPage();
                    CurrentTitle = resourceLoader.GetString(Constants.MusicNavigation);
                    break;
                case "3":
                    CurrentContent = new InformationPage();
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
                            CurrentContent = new GoogleDrivePage();
                        }
                    }
                    else
                    {
                        CurrentContent = new GoogleDrivePage();
                    }
                    googleDrivePage = currentContent;
                    CurrentTitle = resourceLoader.GetString(Constants.GoogleDriveNav);
                    break;
                case "5":
                    CurrentContent = new FtpPage();
                    CurrentTitle = resourceLoader.GetString(Constants.FtpServer);
                    break;
                default:
                    CurrentContent = new MainTitlePage();
                    CurrentTitle = resourceLoader.GetString(Constants.MainNavigation);
                    break;
            }
        }
    }
}
