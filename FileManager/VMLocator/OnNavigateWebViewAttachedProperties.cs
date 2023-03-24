using System.Windows.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace FileManager.VMLocator
{
    public static class NavigationStartingAttachedProperties
    {
        public static DependencyProperty NavigationStartingCommandProperty =
       DependencyProperty.RegisterAttached("NavigationStartingCommand",
           typeof(ICommand),
           typeof(NavigationStartingAttachedProperties),
           new PropertyMetadata(null, OnNavigationStartingCommandChanged));

        public static void SetNavigationStartingCommand(DependencyObject target, ICommand value)
        {
            target?.SetValue(NavigationStartingCommandProperty, value);
        }

        public static ICommand GetNavigationStartingCommand(DependencyObject target)
        {
            return (ICommand)target?.GetValue(NavigationStartingCommandProperty);
        }

        private static void OnNavigationStartingCommandChanged(DependencyObject target,
            DependencyPropertyChangedEventArgs e)
        {
            var element = (WebView)target;
            if (element != null)
            {
                if ((e.NewValue != null) && (e.OldValue == null))
                {
                    element.NavigationStarting += OnNavigationStartingChanged;
                }

                else if ((e.NewValue == null) && (e.OldValue != null))
                {
                    element.NavigationStarting -= OnNavigationStartingChanged;
                }
            }
        }

        private static void OnNavigationStartingChanged(object sender, WebViewNavigationStartingEventArgs e)
        {
            GetNavigationStartingCommand(sender as WebView).Execute(e);
        }
    }
}
