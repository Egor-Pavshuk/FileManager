using FileManager.Views;
using System;
using Windows.ApplicationModel.Resources;
using Windows.UI.Xaml.Controls;

namespace FileManager.ViewModels
{
    public class MainViewModel : BindableBase
    {
        private const string ImageNavigation = "ImageNav";
        private const string VideoNavigation = "VideoNav";
        private const string MusicNavigation = "MusicNav";
        private const string InfoNavigation = "InformationNav";
        private const string MainNavigation = "MainPage";
        private const string GoogleDriveNav = "GoogleDriveNav";
        private const string Resourses = "Resources";
        private const string MainPage = "MainPage";
        private const string FtpServer = "ftpServer";
        private const string GoogleDriveIconPath = "ms-appx:///Images/googleDrive.png";
        private const string FtpIconPath = "ms-appx:///Images/ftpFolder.png";
        private Page currentContent;
        private Page googleDrivePage;
        private string currentTitle;
        private ResourceLoader resourceLoader;
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
            resourceLoader = ResourceLoader.GetForCurrentView(Resourses);
            CurrentTitle = resourceLoader.GetString(MainPage);
            GoogleDriveIconUri = new Uri(GoogleDriveIconPath);
            FtpIconUri = new Uri(FtpIconPath);
        }

        private void SelectionChanged()
        {
            switch (selectedItem.AccessKey)
            {
                case "0":
                    CurrentContent = new PicturesLibraryPage();
                    CurrentTitle = resourceLoader.GetString(ImageNavigation);
                    break;
                case "1":
                    CurrentContent = new VideosLibraryPage();
                    CurrentTitle = resourceLoader.GetString(VideoNavigation);
                    break;
                case "2":
                    CurrentContent = new MusicsLibraryPage();
                    CurrentTitle = resourceLoader.GetString(MusicNavigation);
                    break;
                case "3":
                    CurrentContent = new InformationPage();
                    CurrentTitle = resourceLoader.GetString(InfoNavigation);
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
                    CurrentTitle = resourceLoader.GetString(GoogleDriveNav);
                    break;
                case "5":
                    CurrentContent = new FtpPage();
                    CurrentTitle = resourceLoader.GetString(FtpServer);
                    break;
                default:
                    CurrentContent = new MainTitlePage();
                    CurrentTitle = resourceLoader.GetString(MainNavigation);
                    break;
            }
        }
    }
}
