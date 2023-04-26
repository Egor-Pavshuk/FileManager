using System;
using System.Collections.Generic;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using Autofac;
using FileManager.ViewModels;
using FileManager.ViewModels.Information;
using FileManager.ViewModels.Libraries;
using FileManager.Views;
using FileManager.Dependencies;
using FileManager.Controlls;
using FileManager.Models.Interfaces;
using ThirdPartyServices.UWP.AuthorizationServices;
using ThirdPartyServices.Shared.Interfaces;
using ThirdPartyServices.UWP.CloudServices;
using ThirdPartyServices.UWP.DataAccess;

namespace FileManager
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    sealed partial class App : Application
    {
        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public static IContainer Container { get; set; }
        public App()
        {
            this.InitializeComponent();
            Container = ConfigureServices();
            VMDependencies.Container = Container;
            this.Suspending += OnSuspending;
        }
        private IContainer ConfigureServices()
        {
            ThirdPartyService.Instance.InitializeThirdPartySevices();
            var containerBuilder = new ContainerBuilder();
            containerBuilder.RegisterType<ContentDialogControlViewModel>()
                .AsSelf().WithParameters(new List<NamedParameter>
                {
                    new NamedParameter("title", "title"),
                    new NamedParameter("placeHolder", "placeHolder"),
                    new NamedParameter("primaryButtonText", "primaryButtonText"),
                    new NamedParameter("secondaryButtonText", "secondaryButtonText"),
                    new NamedParameter("inputText", "inputText"),
                });
            containerBuilder.RegisterType<PicturesLibraryViewModel>()
                .AsSelf();
            containerBuilder.RegisterType<VideosLibraryViewModel>()
                .AsSelf();
            containerBuilder.RegisterType<MusicsLibraryViewModel>()
                .AsSelf();            
            containerBuilder.RegisterType<MainViewModel>()
                .AsSelf();
            containerBuilder.RegisterType<MainTitleViewModel>()
                .AsSelf();
            containerBuilder.RegisterType<GoogleDriveViewModel>()
                .AsSelf();
            containerBuilder.RegisterType<FtpViewModel>()
                .AsSelf();
            containerBuilder.RegisterType<InformationViewModel>()
                .AsSelf();
            containerBuilder.RegisterType<OneDriveViewModel>()
                .AsSelf();
            containerBuilder.RegisterType<MainPage>().AsSelf();
            containerBuilder.RegisterType<MainTitlePage>().AsSelf();
            containerBuilder.RegisterType<GoogleDrivePage>().AsSelf();
            containerBuilder.RegisterType<ContentDialogControl>().AsSelf();
            containerBuilder.RegisterType<PicturesLibraryPage>().AsSelf();
            containerBuilder.RegisterType<VideosLibraryPage>().AsSelf();
            containerBuilder.RegisterType<MusicsLibraryPage>().AsSelf();
            containerBuilder.RegisterType<InformationPage>().AsSelf();
            containerBuilder.RegisterType<FtpPage>().AsSelf();
            containerBuilder.RegisterType<OneDrivePage>().AsSelf();

            containerBuilder.RegisterType<WebViewDialog>().As<IAuthWebViewDialog>();
            containerBuilder.RegisterType<MicrosoftAuthService>().As<IMicrosoftAuthorizationService>();
            containerBuilder.RegisterType<OneDriveCloudService>().As<IOneDriveCloudService>();
            VMDependencies.ConfigureServices(typeof(MainPage), typeof(MainTitlePage), typeof(FtpPage), typeof(GoogleDrivePage),
                typeof(InformationPage), typeof(MusicsLibraryPage), typeof(PicturesLibraryPage), typeof(VideosLibraryPage),
                typeof(ContentDialogControl), typeof(OneDrivePage));
            var container = containerBuilder.Build();
            return container;
        }
        /// <summary>
        /// Invoked when the application is launched normally by the end user.  Other entry points
        /// will be used such as when the application is launched to open a specific file.
        /// </summary>
        /// <param name="e">Details about the launch request and process.</param>
        protected override void OnLaunched(LaunchActivatedEventArgs e)
        {
            Frame rootFrame = Window.Current.Content as Frame;

            // Do not repeat app initialization when the Window already has content,
            // just ensure that the window is active
            if (rootFrame == null)
            {
                // Create a Frame to act as the navigation context and navigate to the first page
                rootFrame = new Frame();

                rootFrame.NavigationFailed += OnNavigationFailed;

                if (e?.PreviousExecutionState == ApplicationExecutionState.Terminated)
                {
                    //TODO: Load state from previously suspended application
                }

                // Place the frame in the current Window
                Window.Current.Content = rootFrame;
            }

            if (e?.PrelaunchActivated == false)
            {
                if (rootFrame.Content == null)
                {
                    // When the navigation stack isn't restored navigate to the first page,
                    // configuring the new page by passing required information as a navigation
                    // parameter
                    rootFrame.Navigate(typeof(MainPage), e.Arguments);
                }
                // Ensure the current window is active
                Window.Current.Activate();
            }
        }

        /// <summary>
        /// Invoked when Navigation to a certain page fails
        /// </summary>
        /// <param name="sender">The Frame which failed navigation</param>
        /// <param name="e">Details about the navigation failure</param>
        void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            throw new Exception("Failed to load Page " + e.SourcePageType.FullName);
        }

        /// <summary>
        /// Invoked when application execution is being suspended.  Application state is saved
        /// without knowing whether the application will be terminated or resumed with the contents
        /// of memory still intact.
        /// </summary>
        /// <param name="sender">The source of the suspend request.</param>
        /// <param name="e">Details about the suspend request.</param>
        private void OnSuspending(object sender, SuspendingEventArgs e)
        {
            var deferral = e.SuspendingOperation.GetDeferral();
            //TODO: Save application state and stop any background activity
            deferral.Complete();
        }
    }
}
