using System.Windows.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace FileManager.VMLocator
{
    public static class SelectionChangedAttachedProperties
    {
        public static DependencyProperty SelectionChangedCommandProperty =
       DependencyProperty.RegisterAttached("SelectionChangedCommand",
           typeof(ICommand),
           typeof(SelectionChangedAttachedProperties),
           new PropertyMetadata(null, OnSelectionChangedCommandChanged));

        public static void SetSelectionChangedCommand(DependencyObject target, ICommand value)
        {
            target?.SetValue(SelectionChangedCommandProperty, value);
        }

        public static ICommand GetSelectionChangedCommand(DependencyObject target)
        {
            return (ICommand)target?.GetValue(SelectionChangedCommandProperty);
        }

        private static void OnSelectionChangedCommandChanged(DependencyObject target,
            DependencyPropertyChangedEventArgs e)
        {
            var element = (ListViewBase)target;
            if (element != null)
            {
                if ((e.NewValue != null) && (e.OldValue == null))
                {
                    element.SelectionChanged += OnSelectionChanged;
                }

                else if ((e.NewValue == null) && (e.OldValue != null))
                {
                    element.SelectionChanged -= OnSelectionChanged;
                }
            }
        }

        private static void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            GetSelectionChangedCommand(sender as ListViewBase).Execute(sender);
        }
    }
}
