using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using Windows.UI.Xaml;
using FileManager.ViewModels;
using FileManager.ViewModels.Information;
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
                    typeof(FileControlViewModel),
                    typeof(OnlineFileControlViewModel),
                    typeof(InformationControlViewModel)
                };
                if (!typesWithoutActivation.Any(t => t == viewModelType))
                {
                    switch (frameworkElement.GetType().Name)
                    {
                        case nameof(PicturesLibraryPage):
                            frameworkElement.DataContext = Activator.CreateInstance(viewModelType, "Pictures");
                            break;
                        case nameof(VideosLibraryPage):
                            frameworkElement.DataContext = Activator.CreateInstance(viewModelType, "Videos");
                            break;
                        case nameof(MusicsLibraryPage):
                            frameworkElement.DataContext = Activator.CreateInstance(viewModelType, "Music");
                            break;
                        default:
                            frameworkElement.DataContext = Activator.CreateInstance(viewModelType);
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
                if (viewType.FullName.Contains("Information", StringComparison.Ordinal))
                {
                    viewName = viewType.FullName
                    .Replace("Controlls", "ViewModels.Information", StringComparison.Ordinal);
                }
                else
                {
                    viewName = viewType.FullName
                    .Replace("Controlls", "ViewModels", StringComparison.Ordinal);
                }
            }

            var viewAssemblyName = viewType.GetTypeInfo().Assembly.FullName;
            var viewModelName = string.Format(CultureInfo.InvariantCulture, "{0}ViewModel, {1}", viewName, viewAssemblyName);

            return Type.GetType(viewModelName);
        }
    }
}
