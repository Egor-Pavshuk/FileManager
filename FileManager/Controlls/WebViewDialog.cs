using FileManager.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml;
using FileManager.Models.Interfaces;

namespace FileManager.Controlls
{
    [TemplatePart(Name = CancelButtonName, Type = typeof(Button))]
    public class WebViewDialog : BaseDialogControl<object>, IAuthWebViewDialog
    {
        #region Template
        private const string CancelButtonName = "CancelButton";

        private Button cancelButton;
        #endregion

        public WebViewDialog() : base()
        {
            DefaultStyleKey = typeof(WebViewDialog);
            Visibility = Visibility.Collapsed;
        }

        protected override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            cancelButton = GetTemplateChild(CancelButtonName) as Button;
        }

        protected override void OnLoaded(object sender, RoutedEventArgs e)
        {
            base.OnLoaded(sender, e);

            if (cancelButton != null)
            {
                cancelButton.Click += OnCancelled;
            }
        }

        private void OnCancelled(object sender, RoutedEventArgs e)
        {
            if (cancelButton != null)
            {
                base.DismissDialog();
            }
        }
    }
}
