using GalaSoft.MvvmLight.Messaging;
using SmartSounder.Tools;
using SmartSounder.ViewModel;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace SmartSounder
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainViewModel MainViewModel => new ViewModelLocator().Main;

        public MainViewModel MainVm => (MainViewModel)DataContext;

        public MainPage()
        {
            this.InitializeComponent();

            SystemNavigationManager.GetForCurrentView().BackRequested += MainPage_BackRequested;

            MainVm.NotifyWithSound += MainVm_NotifyWithSound;

            this.Loaded += MainPage_Loaded;
        }

        private void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
        }

        private async void Current_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            StorageFile file = await ApplicationData.Current.LocalFolder.CreateFileAsync("info.log", CreationCollisionOption.OpenIfExists);
            using (var stream = await file.OpenAsync(FileAccessMode.ReadWrite))
            {
                stream.Seek(stream.Size);
                StreamWriter writer = new StreamWriter(stream.AsStream());
                writer.Write(MainVm.LogStr);
                writer.Flush();
            }

        }

        private async void MainVm_NotifyWithSound(object sender, NotifyWithSoundEventArgs args)
        {
            await this.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
            {
                mediaElement.MediaEnded -= MediaElement_MediaEnded;
                switch (args.NoticeType)
                {
                    case NoticeType.Ready:
                        mediaElement.Stop();
                        mediaElement.AutoPlay = true;
                        mediaElement.IsLooping = false;
                        mediaElement.Source = new Uri("ms-appx:///Assets/Audios/listening.wav", UriKind.Absolute);
                        mediaElement.Play();
                        break;
                    case NoticeType.Completed:
                        mediaElement.Stop();
                        mediaElement.AutoPlay = true;
                        mediaElement.IsLooping = false;
                        mediaElement.MediaEnded += MediaElement_MediaEnded;
                        mediaElement.Source = new Uri("ms-appx:///Assets/Audios/results.wav", UriKind.Absolute);
                        mediaElement.Play();
                        break;
                    case NoticeType.Thinking:
                        mediaElement.Stop();
                        mediaElement.AutoPlay = true;
                        mediaElement.IsLooping = true;
                        mediaElement.Source = new Uri("ms-appx:///Assets/Audios/processing.wav", UriKind.Absolute);
                        mediaElement.Play();
                        break;
                    case NoticeType.Failed:
                        mediaElement.Stop();
                        mediaElement.AutoPlay = true;
                        mediaElement.IsLooping = false;
                        mediaElement.MediaEnded += MediaElement_MediaEnded;
                        mediaElement.Source = new Uri("ms-appx:///Assets/Audios/canceled.wav", UriKind.Absolute);
                        mediaElement.Play();
                        break;
                    case NoticeType.Speech:
                        mediaElement.Stop();
                        mediaElement.AutoPlay = true;
                        mediaElement.IsLooping = false;
                        mediaElement.MediaEnded += MediaElement_MediaEnded;
                        mediaElement.MediaFailed += MediaElement_MediaFailed;
                        var speechStream = await SpeechSynthesisHelper.TextToSpeechAsync(args.Text);
                        mediaElement.SetSource(speechStream, speechStream.ContentType);
                        mediaElement.Play();
                        break;
                    default:
                        break;
                }
            });

        }

        private void MediaElement_MediaFailed(object sender, ExceptionRoutedEventArgs e)
        {

        }

        private void MediaElement_MediaEnded(object sender, RoutedEventArgs e)
        {
            mediaElement.MediaEnded -= MediaElement_MediaEnded;
            MainVm.SpeechAndMusicResetEvent.Set();
        }

        private void MainPage_BackRequested(object sender, BackRequestedEventArgs e)
        {
            if (Frame.CanGoBack)
            {
                e.Handled = true;
                Frame.GoBack();
            }
        }

        private void TextBlock_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            sv.ScrollToVerticalOffset(tbLog.ActualHeight);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            throw new Exception("手动异常");
        }
    }
}
