using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Windows.UI.Xaml;
using Autofac;
using FileManager.Dependencies;
using FileManager.Helpers;
using FileManager.ViewModels.Libraries;

namespace FileManager.ViewModels.VMLocator
{
    public static class ViewModelLocator
    {
        public static DependencyProperty AutoWireViewModelProperty = DependencyProperty.RegisterAttached("AutoWireViewModel", typeof(bool),
        typeof(ViewModelLocator), new PropertyMetadata(false, AutoWireViewModelChanged));

        public static bool GetAutoWireViewModel(UIElement element)
        {
            return (bool)element?.GetValue(AutoWireViewModelProperty);
        }

        public static void SetAutoWireViewModel(UIElement element, bool value)
        {
            element?.SetValue(AutoWireViewModelProperty, value);
        }

        private static void AutoWireViewModelChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if ((bool)e.NewValue)
            {
                Bind(d);
            }
        }

        private static void Bind(DependencyObject view)
        {
            if (view is FrameworkElement frameworkElement)
            {
                var viewModelType = FindViewModel(frameworkElement.GetType());
                var typesWithoutActivation = new List<Type>()
                {
                    VMDependencies.Views[Constants.FileControl],
                    VMDependencies.Views[Constants.OnlineFileControl],
                    VMDependencies.Views[Constants.InformationControl],
                };
                if (!typesWithoutActivation.Any(t => t == frameworkElement.GetType()))
                {
                    switch (frameworkElement.GetType().Name)
                    {
                        case Constants.PicturesLibraryPage:
                            frameworkElement.DataContext = VMDependencies.Container.Resolve<PicturesLibraryViewModel>();
                            break;
                        case Constants.VideosLibraryPage:
                            frameworkElement.DataContext = VMDependencies.Container.Resolve<VideosLibraryViewModel>();
                            break;
                        case Constants.MusicsLibraryPage:
                            frameworkElement.DataContext = VMDependencies.Container.Resolve<MusicsLibraryViewModel>();
                            break;
                        default:
                            frameworkElement.DataContext = VMDependencies.Container.Resolve(viewModelType);
                            break;
                    }
                }
            }
        }

        private static Type FindViewModel(Type viewType)
        {
            string viewName = string.Empty;

            if (viewType.FullName.EndsWith("Page"))
            {
                viewName = viewType.FullName
                    .Replace("Page", string.Empty, StringComparison.Ordinal)
                    .Replace("Views", "ViewModels", StringComparison.Ordinal);

                if (viewType.FullName.Contains("Information", StringComparison.Ordinal))
                {
                    viewName = viewName
                    .Replace("ViewModels", "ViewModels.Information", StringComparison.Ordinal);
                }
                if (viewName.Contains("Library", StringComparison.Ordinal))
                {
                    viewName = viewName.Replace(viewName.Substring(viewName.LastIndexOf('.') + 1), "Libraries.LibrariesBase", StringComparison.Ordinal);
                }
            }
            else if (viewType.FullName.EndsWith("Control"))
            {
                viewName = viewType.FullName
                    .Replace("Controlls", "ViewModels", StringComparison.Ordinal);
            }

            var viewAssemblyName = "FileManager.ViewModels";
            var viewModelName = string.Format(CultureInfo.InvariantCulture, "{0}ViewModel, {1}", viewName, viewAssemblyName);
            return Type.GetType(viewModelName);
        }
    }
}
