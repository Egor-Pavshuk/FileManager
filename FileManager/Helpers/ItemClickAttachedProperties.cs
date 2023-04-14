using System.Windows.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace FileManager.Helpers
{
    public static class ItemClickAttachedProperties
    {
        public static DependencyProperty ItemClickCommandProperty =
       DependencyProperty.RegisterAttached("ItemClickCommand",
           typeof(ICommand),
           typeof(ItemClickAttachedProperties),
           new PropertyMetadata(null, OnItemClickCommandChanged));

        public static void SetItemClickCommand(DependencyObject target, ICommand value)
        {
            target?.SetValue(ItemClickCommandProperty, value);
        }

        public static ICommand GetItemClickCommand(DependencyObject target)
        {
            return (ICommand)target?.GetValue(ItemClickCommandProperty);
        }

        private static void OnItemClickCommandChanged(DependencyObject target,
            DependencyPropertyChangedEventArgs e)
        {
            var element = (ListViewBase)target;
            if (element != null)
            {
                if ((e.NewValue != null) && (e.OldValue == null))
                {
                    element.ItemClick += OnItemClick;
                }

                else if ((e.NewValue == null) && (e.OldValue != null))
                {
                    element.ItemClick -= OnItemClick;
                }
            }
        }

        private static void OnItemClick(object sender, ItemClickEventArgs e)
        {
            GetItemClickCommand(sender as ListViewBase).Execute(sender);
        }
    }
}
