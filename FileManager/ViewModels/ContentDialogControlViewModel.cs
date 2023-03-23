﻿using System.Windows.Input;

namespace FileManager.ViewModels
{
    public class ContentDialogControlViewModel : BindableBase
    {
        private string inputText;
        private string title;
        private string placeHolder;
        private string primaryButtonText;
        private string secondaryButtonText;
        private ICommand primaryButtonCommand;
        private ICommand secondaryButtonCommand;
        public string InputText
        {
            get => inputText;
            set
            {
                if (inputText != value)
                {
                    inputText = value;
                    OnPropertyChanged();
                }
            }
        }
        public string Title
        {
            get => title;
            set
            {
                if (title != value)
                {
                    title = value;
                    OnPropertyChanged();
                }
            }
        }
        public string PlaceHolder
        {
            get => placeHolder;
            set
            {
                if (placeHolder != value)
                {
                    placeHolder = value;
                    OnPropertyChanged();
                }
            }
        }
        public string PrimaryButtonContent
        {
            get => primaryButtonText;
            set
            {
                if (primaryButtonText != value)
                {
                    primaryButtonText = value;
                    OnPropertyChanged();
                }
            }
        }
        public string SecondaryButtonContent
        {
            get => secondaryButtonText;
            set
            {
                if (secondaryButtonText != value)
                {
                    secondaryButtonText = value;
                    OnPropertyChanged();
                }
            }
        }
        public ICommand PrimaryButtonCommand
        {
            get => primaryButtonCommand;
            set
            {
                if (primaryButtonCommand != value)
                {
                    primaryButtonCommand = value;
                    OnPropertyChanged();
                }
            }
        }
        public ICommand SecondaryButtonCommand
        {
            get => secondaryButtonCommand;
            set
            {
                if (secondaryButtonCommand != value)
                {
                    secondaryButtonCommand = value;
                    OnPropertyChanged();
                }
            }
        }
        public ContentDialogControlViewModel()
        {
            Title = string.Empty;
            PlaceHolder = string.Empty;
        }
        public ContentDialogControlViewModel(string title, string placeHolder)
        {
            Title = title;
            PlaceHolder = placeHolder;
        }
    }
}