﻿using Windows.ApplicationModel.Resources;
using Windows.UI.Xaml.Controls;
using Autofac;
using FileManager.Dependencies;
using FileManager.Helpers;

namespace FileManager.ViewModels
{
    public class MainViewModel : BindableBase
    {
        private readonly ResourceLoader resourceLoader;
        private Page currentContent;
        private Page googleDrivePage;
        private string currentTitle;
        private NavigationViewItem selectedItem;

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

        public MainViewModel()
        {
            currentContent = (Page)VMDependencies.Container.Resolve(VMDependencies.Views[Constants.MainTitlePage]);
            resourceLoader = ResourceLoader.GetForCurrentView(Constants.StringResources);
            CurrentTitle = resourceLoader.GetString(Constants.MainPage);
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
                            CurrentContent = (Page)VMDependencies.Container.Resolve(VMDependencies.Views[Constants.GoogleDrivePage]);
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
