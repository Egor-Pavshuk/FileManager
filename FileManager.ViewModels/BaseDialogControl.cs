using CommonToolkit.Core.Extensions;
using FileManager.Helpers;
using FileManager.Models.Dialogs;
using FileManager.Models.Enums;
using FileManager.Models.Interfaces;
using System;
using System.Linq;
using System.Threading.Tasks;
using Windows.System;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Media;

namespace FileManager.ViewModels
{
    public class BaseDialogControl<T> : Control, IDialog<T>
    {
        protected const string PopupStatesGroupName = "PopupStates";
        protected const string OpenPopupStateName = "OpenPopupState";
        protected const string ClosedPopupStateName = "ClosedPopupState";

        protected const string LayoutPanelName = "LayoutRoot";
        protected const string ContentBorderName = "ContentBorder";
        protected const string ContentGridName = "ContentGrid";

        protected const string CloseButtonName = "CloseButton";

        private bool isShowing;

        private Panel parentPanel;
        private Popup dialogPopup;

        private Button closeButton;

        private Color originalColor;

        private ApplicationView AppView = ApplicationView.GetForCurrentView();

        public BaseDialogControl()
        {
            this.Visibility = Visibility.Collapsed;

            this.Loaded += this.OnLoaded;
            this.Unloaded += this.OnUnloaded;
        }

        protected Panel LayoutPanel { get; private set; }

        protected TaskCompletionSource<bool> ApplyTemplateTaskSource { get; } = new TaskCompletionSource<bool>();

        protected TaskCompletionSource<DialogResult> DismissTaskSource { get; private set; }

        protected virtual void OnUnloaded(object sender, RoutedEventArgs e)
        {
            if (this.closeButton != null)
            {
                this.closeButton.Click -= this.OnCloseButtonClick;
            }
        }

        protected virtual async void OnLoaded(object sender, RoutedEventArgs e)
        {
            await this.ApplyTemplateTaskSource.Task;

            if (this.closeButton != null)
            {
                this.closeButton.Click += this.OnCloseButtonClick;
            }
        }

        protected override void OnApplyTemplate()
        {
            this.LayoutPanel = this.GetTemplateChild(LayoutPanelName) as Panel;
            this.closeButton = this.GetTemplateChild(CloseButtonName) as Button;

            base.OnApplyTemplate();

            this.ApplyTemplateTaskSource.TrySetResult(true);
        }

        public virtual async Task<DialogResult> ShowAsync(T param)
        {
            if (this.isShowing)
            {
                throw new InvalidOperationException("The dialog is already shown.");
            }

            //originalColor = AppView.TitleBar.ButtonForegroundColor.Value;
            //AppView.TitleBar.ButtonForegroundColor = Color.FromArgb(0, 255, 255, 255);

            this.Visibility = Visibility.Visible;
            this.isShowing = true;

            Window.Current.CoreWindow.KeyDown += this.OnCoreWindowKeyDown;

            this.DismissTaskSource = new TaskCompletionSource<DialogResult>();

            this.dialogPopup = new Popup { Child = this };

            this.parentPanel = Window.Current.Content.GetFirstDescendantOfType<Panel>();
            if (this.parentPanel != null)
            {
                this.parentPanel.Children.Add(this.dialogPopup);
                this.parentPanel.SizeChanged += this.OnParentSizeChanged;
            }

            this.dialogPopup.IsOpen = true;

            this.LayoutUpdated += this.OnLayoutUpdated;

            var result = await this.DismissTaskSource.Task;

            await this.CloseAsync();

            Window.Current.CoreWindow.KeyDown -= this.OnCoreWindowKeyDown;

            return result;
        }

        public void Dismiss()
        {
            this.DismissDialog();
        }

        private async void OnLayoutUpdated(object sender, object e)
        {
            this.LayoutUpdated -= this.OnLayoutUpdated;
            this.ResizeLayoutRoot();

            this.SetAdditionalLayout();

            await this.GoToVisualStateAsync(this.LayoutPanel, PopupStatesGroupName, OpenPopupStateName);

            // set focus to opened dialog
            this.Focus(FocusState.Programmatic);
        }

        protected virtual void SetAdditionalLayout() { }

        protected void ResizeLayoutRoot()
        {
            if (this.parentPanel != null && this.LayoutPanel != null)
            {
                this.LayoutPanel.Width = this.parentPanel.ActualWidth;
                this.LayoutPanel.Height = this.parentPanel.ActualHeight;
            }
        }

        protected void InvertResizeLayoutRoot()
        {
            if (this.parentPanel != null && this.LayoutPanel != null)
            {
                this.LayoutPanel.Width = this.parentPanel.ActualHeight;
                this.LayoutPanel.Height = this.parentPanel.ActualWidth;
            }
        }

        private void OnParentSizeChanged(object sender, SizeChangedEventArgs sizeChangedEventArgs)
        {
            if (sizeChangedEventArgs.NewSize != sizeChangedEventArgs.PreviousSize)
            {
                this.ResizeLayoutRoot();
            }
        }

        protected virtual void OnCloseButtonClick(object sender, RoutedEventArgs e)
        {
            this.DismissDialog();
        }

        private void OnCoreWindowKeyDown(CoreWindow sender, KeyEventArgs e)
        {
            if (!e.Handled)
            {
                var lastDailog = VisualTreeHelper.GetOpenPopups(Window.Current).FirstOrDefault(popup => popup.Child is IDialog)?.Child as IDialog;
                if (e.VirtualKey == VirtualKey.Escape)
                {
                    lastDailog?.Dismiss();
                }
                else if (e.VirtualKey == VirtualKey.Enter)
                {
                    lastDailog?.OnEnterKeyUp();
                }

                e.Handled = true;
            }
        }

        public virtual void OnEnterKeyUp() { }

        protected virtual void DismissDialog()
        {
            this.DismissTaskSource.TrySetResult(new DialogResult { OperationResult = OperationResult.Canceled });

            AppView.TitleBar.ButtonForegroundColor = originalColor;
        }

        protected virtual async Task CloseAsync()
        {
            if (!this.isShowing)
            {
                throw new InvalidOperationException("The dialog isn't shown, so it can't be closed.");
            }

            await this.GoToVisualStateAsync(this.LayoutPanel, PopupStatesGroupName, ClosedPopupStateName);

            this.dialogPopup.IsOpen = false;
            this.dialogPopup.Child = null;

            if (this.parentPanel != null)
            {
                this.parentPanel.Children.Remove(this.dialogPopup);
                this.parentPanel.SizeChanged -= this.OnParentSizeChanged;
                this.parentPanel = null;
            }

            this.dialogPopup = null;
            this.Visibility = Visibility.Collapsed;
            this.isShowing = false;

            AppView.TitleBar.ButtonForegroundColor = originalColor;
        }
    }
}