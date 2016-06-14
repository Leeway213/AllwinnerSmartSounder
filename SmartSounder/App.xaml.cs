using BackgroundAudioProtocol.BackgroundAudioSettings;
using GalaSoft.MvvmLight.Threading;
using IntelligentService;
using SmartSounder.Tools;
using SmartSounder.Tools.RemoteControl;
using SmartSounder.ViewModel;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using System.Threading.Tasks;
using UDPService;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace SmartSounder
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
        public App()
        {
            Microsoft.ApplicationInsights.WindowsAppInitializer.InitializeAsync(
                Microsoft.ApplicationInsights.WindowsCollectors.Metadata |
                Microsoft.ApplicationInsights.WindowsCollectors.Session);
            this.InitializeComponent();
            this.Suspending += OnSuspending;
            this.UnhandledException += App_UnhandledException;
        }

        private async void App_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Debug.WriteLine(e.Message);
            //ViewModelLocator locator = new ViewModelLocator();
            //string logStr = locator.Main.LogStr;

            //AutoResetEvent resetEvent = new AutoResetEvent(false);
            //await Task.Factory.StartNew(() =>
            //{
            //    var fileOpera= ApplicationData.Current.LocalFolder.CreateFileAsync("log", CreationCollisionOption.GenerateUniqueName);
            //    var file = fileOpera.GetResults();
            //    var streamOpera = file.OpenAsync(FileAccessMode.ReadWrite);
            //    using (var stream = streamOpera.GetResults())
            //    {
            //        StreamWriter writer = new StreamWriter(stream.AsStream());
            //        writer.Write(logStr);
            //        string log = DateTime.Now.ToString() + ":  " + e.Exception.ToString() + "__" + e.Message;
            //        writer.WriteLine(log);
            //        writer.Flush();
            //    }

            //UDPServer.Current?.SaveClientList();

            //    resetEvent.Set();
            //});


            //resetEvent.WaitOne(5000);
        }

        public static Frame MainFrame;
        /// <summary>
        /// Invoked when the application is launched normally by the end user.  Other entry points
        /// will be used such as when the application is launched to open a specific file.
        /// </summary>
        /// <param name="e">Details about the launch request and process.</param>
        protected override void OnLaunched(LaunchActivatedEventArgs e)
        {
            //#if DEBUG
            //            if (System.Diagnostics.Debugger.IsAttached)
            //            {
            //                this.DebugSettings.EnableFrameRateCounter = true;
            //            }
            //#endif
            //Frame rootFrame = Window.Current.Content as Frame;
            MainFrame = Window.Current.Content as Frame;

            // Do not repeat app initialization when the Window already has content,
            // just ensure that the window is active
            if (MainFrame == null)
            {
                // Create a Frame to act as the navigation context and navigate to the first page
                MainFrame = new Frame();

                MainFrame.NavigationFailed += OnNavigationFailed;

                if (e.PreviousExecutionState == ApplicationExecutionState.Terminated)
                {
                    //TODO: Load state from previously suspended application
                }

                // Place the frame in the current Window
                Window.Current.Content = MainFrame;
            }

            if (e.PrelaunchActivated == false)
            {
                if (MainFrame.Content == null)
                {
                    // When the navigation stack isn't restored navigate to the first page,
                    // configuring the new page by passing required information as a navigation
                    // parameter
                    MainFrame.Navigate(typeof(MainPage), e.Arguments);
                }
                // Ensure the current window is active
                Window.Current.Activate();
                BackgroundAudioSettingsHelper.SetValue(BackgroundAudioSettingsConstants.APP_STATE, AppState.Active.ToString());
                try
                {
                    UDPServer.StartService("14288");

                    RemoteControlService.StartService();
                }
                catch (Exception ex)
                {
                    throw new Exception("启动UDP服务失败", ex);
                }
            }

            DispatcherHelper.Initialize();

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
