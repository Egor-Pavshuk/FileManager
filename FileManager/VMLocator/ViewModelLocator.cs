using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Windows.UI.Xaml;
using Autofac;
using FileManager.Controlls;
using FileManager.ViewModels.Libraries;
using FileManager.Views;

namespace FileManager.VMLocator
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
                    typeof(FileControl),
                    typeof(OnlineFileControl),
                    typeof(InformationControl)
                };
                if (!typesWithoutActivation.Any(t => t == frameworkElement.GetType()))
                {
                    switch (frameworkElement.GetType().Name)
                    {
                        case nameof(PicturesLibraryPage):
                            frameworkElement.DataContext = App.Container.Resolve<PicturesLibraryViewModel>();
                            break;
                        case nameof(VideosLibraryPage):
                            frameworkElement.DataContext = App.Container.Resolve<VideosLibraryViewModel>();
                            break;
                        case nameof(MusicsLibraryPage):
                            frameworkElement.DataContext = App.Container.Resolve<MusicsLibraryViewModel>();
                            break;
                        default:
                            frameworkElement.DataContext = App.Container.Resolve(viewModelType);
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
