using System.Windows.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

namespace FileManager.Helpers
{
    public static class DoubleTapAttachedProperties
    {
        public static DependencyProperty ItemDoubleTapCommandProperty =
       DependencyProperty.RegisterAttached("ItemDoubleTapCommand",
           typeof(ICommand),
           typeof(DoubleTapAttachedProperties),
           new PropertyMetadata(null, OnItemDoubleTapCommandChanged));

        public static void SetItemDoubleTapCommand(DependencyObject target, ICommand value)
        {
            target?.SetValue(ItemDoubleTapCommandProperty, value);
        }

        public static ICommand GetItemDoubleTapCommand(DependencyObject target)
        {
            return (ICommand)target?.GetValue(ItemDoubleTapCommandProperty);
        }

        private static void OnItemDoubleTapCommandChanged(DependencyObject target,
            DependencyPropertyChangedEventArgs e)
        {
            var element = (ListViewBase)target;
            if (element != null)
            {
                if ((e.NewValue != null) && (e.OldValue == null))
                {
                    element.DoubleTapped += OnItemDoubleTap;
                }

                else if ((e.NewValue == null) && (e.OldValue != null))
                {
                    element.DoubleTapped -= OnItemDoubleTap;
                }
            }
        }

        private static void OnItemDoubleTap(object sender, DoubleTappedRoutedEventArgs e)
        {
            GetItemDoubleTapCommand(sender as ListViewBase).Execute(sender);
        }
    }
}
