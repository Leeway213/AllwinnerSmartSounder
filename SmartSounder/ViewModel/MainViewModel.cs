using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Views;
using SmartSounder.Model;
using SmartSounder.Tools;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources.Core;
using Windows.Globalization;
using Windows.Media.SpeechRecognition;
using Windows.UI;
using Windows.UI.Xaml.Media;
using IntelligentService.Models;
using UWPHelpers;
using Windows.Media.Playback;
using BackgroundAudioProtocol.Models;
using BackgroundAudioProtocol.Messages;
using System.Threading;
using SmartSounder.Tools.Weather;
using SmartSounder.Tools.AppResources;
using TulingRobotService;
using TulingRobotService.Models;
using Windows.Storage;
using System.IO;
using UDPService;
using SmartSounder.Tools.WlanConnect;
using GalaSoft.MvvmLight.Threading;
using SmartSounder.Tools.SpeechRecognizers;

namespace SmartSounder.ViewModel
{
    public class NotifyWithSoundEventArgs
    {
        public NoticeType NoticeType { get; set; }

        public string Text { get; set; }
    }

    /// <summary>
    /// 通知类型（Ready、Failed、Completed、Thinking、Speech）
    /// </summary>
    public enum NoticeType
    {
        Ready,
        Failed,
        Completed,
        Thinking,
        Speech
    }

    public class MainViewModel : ViewModelBase
    {
        #region ResetEvent for thread sync

        /// <summary>
        /// 语音和音乐线程同步的等待事件
        /// </summary>
        public AutoResetEvent SpeechAndMusicResetEvent = new AutoResetEvent(false);

        /// <summary>
        /// Thinking超时时的等待事件
        /// </summary>
        public AutoResetEvent ThinkResetEvent = new AutoResetEvent(false);

        /// <summary>
        /// 同步两个SpeechRecognizer时使用的的等待事件
        /// </summary>
        public AutoResetEvent DualSpeeckRecResetEvent = new AutoResetEvent(false);

        #endregion

        #region Private Field

        private const uint HResultRecognizerNotFound = 0x8004503a;
        private readonly INavigationService _navigationService;
        private readonly IMainDataService _mainDataService;


        private object lockObj = new object();

        private CancellationTokenSource _cancellationTokenSource;

        /// <summary>
        /// 语音唤醒时使用的SpeechRecognizer
        /// </summary>
        //private SpeechRecognizer _speechRecognizerForWakeUp;

        /// <summary>
        /// 识别命令时使用的SpeechRecognizer
        /// </summary>
        //private SpeechRecognizer _speechRecognizerForCommand;

        /// <summary>
        /// 离线使用的SpeechRecognizer
        /// </summary>
        private SpeechRecognizer _speechRecognizerForOffline;

        /// <summary>
        /// 语音唤醒使用的资源map
        /// </summary>
        private ResourceMap _speechResourceMap;

        /// <summary>
        /// 语音唤醒使用的资源上下文
        /// </summary>
        private ResourceContext _speechContext;

        /// <summary>
        /// 是否正在初始化SpeechRecognizer
        /// </summary>
        private bool isInitializing;

        /// <summary>
        /// 是否正在播放音乐
        /// </summary>
        private bool isPlaying;

        /// <summary>
        /// 当前使用的是否是prebuilt LUIS服务
        /// </summary>
        bool isPrebuiltApp = true;

        /// <summary>
        /// 是否捕捉到一些命令
        /// </summary>
        bool catchedSomething = false;

        #endregion

        #region Properties

        public bool IsOnline
        {
            get { return NetworkHelper.CheckNetworkAvailability(); }
        }


        private string _logStr;

        public string LogStr
        {
            get { return _logStr; }
            set { Set(ref _logStr, value); }
        }


        /// <summary>
        /// 暂未使用
        /// </summary>
        public MediaPlayer Player
        {
            get
            {
                return MediaController.Current.CurrentPlayer;
            }
        }

        private string _commandRecStatus;

        public string CommandRecStatus
        {
            get { return _commandRecStatus; }
            set { Set(ref _commandRecStatus, value); }
        }


        private ObservableCollection<Language> _languages;

        /// <summary>
        /// 系统支持的语音类型集合
        /// </summary>
        public ObservableCollection<Language> Languages
        {
            get
            {
                return _languages;
            }
            set
            {
                Set(ref _languages, value);
            }
        }

        private string _text;

        /// <summary>
        /// 实时语音识别文本
        /// </summary>
        public string Text
        {
            get
            {
                return _text;
            }
            set
            {
                Set(ref _text, value);
            }
        }

        private SolidColorBrush _statusColor;

        /// <summary>
        /// 状态指示颜色
        /// </summary>
        public SolidColorBrush StatusColor
        {
            get
            {
                return _statusColor;
            }
            set
            {
                Set(ref _statusColor, value);
            }
        }

        private int _selectedLanguageIndex;

        /// <summary>
        /// 当前语言
        /// </summary>
        public int SelectedLanguageIndex
        {
            get
            {
                return _selectedLanguageIndex;
            }
            set
            {
                Set(ref _selectedLanguageIndex, value);

                //ChangeLanguange(value);
            }
        }

        private string _speechStatus;

        /// <summary>
        /// 语音识别器状态文本
        /// </summary>
        public string SpeechStatus
        {
            get { return _speechStatus; }
            set { Set(ref this._speechStatus, value); }
        }

        private string _noneStatus;
        public string NoneStatus
        {
            get
            {
                return _noneStatus;
            }
            set
            {
                Set(ref _noneStatus, value);
            }
        }

        private ObservableCollection<SongModel> _playlist;

        public ObservableCollection<SongModel> Playlist
        {
            get { return _playlist; }
            set { Set(ref _playlist, value); }
        }

        private int _currentIndex;

        public int CurrentIndex
        {
            get { return _currentIndex; }
            set
            {
                if (_currentIndex != value)
                {
                    Set(ref _currentIndex, value);
                    MediaController.Current.CurrentPlayingIndex = value;
                }
            }
        }



        #endregion

        #region Event

        public delegate void NotifyWithSoundEventHandler(object sender, NotifyWithSoundEventArgs args);
        /// <summary>
        /// 通知前台media播放器播放提示音的事件
        /// </summary>
        public event NotifyWithSoundEventHandler NotifyWithSound;

        #endregion

        #region Private Methods

        public async void Log(string log)
        {
            await App.MainFrame.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, async () =>
            {
                LogStr += DateTime.Now.ToString() + ":  " + log + "\n";
                if (LogStr.Length > 8000)
                {
                    StringBuilder sb = new StringBuilder(LogStr);
                    LogStr = "";
                    StorageFile logfile = await ApplicationData.Current.LocalFolder.CreateFileAsync("log", CreationCollisionOption.GenerateUniqueName);
                    using (var stream = await logfile.OpenAsync(FileAccessMode.ReadWrite))
                    {
                        StreamWriter writer = new StreamWriter(stream.AsStream());
                        writer.Write(sb.ToString());
                        writer.Flush();
                    }
                }

            });
            Debug.WriteLine(log);
        }

        /// <summary>
        /// 初始化本地媒体内容，更新后台播放列表
        /// </summary>
        private async void InitializeLocalMedia()
        {
            MediaController.Current.MessageReceivedFromBackground += Current_MessageReceivedFromBackground;

            Playlist = new ObservableCollection<SongModel>();

            //获取本地音乐信息，等待后台任务启动后，更新播放列表
            var musicsProperties = await LocalMusicHelper.ScanLocalMusicInLibrary();
            foreach (var properties in musicsProperties)
            {
                MediaController.Current.SongList.Add(new SongModel()
                {
                    Title = properties.DefinedProperties.Title,
                    FileName = properties.FileName,
                    Artist = properties.DefinedProperties.Artist,
                    Genre = properties.DefinedProperties.Genre.ToArray(),
                    Duration = properties.DefinedProperties.Duration
                });
            }


            MediaController.Current.PlaylistChanged += Current_PlaylistChanged;

            bool result = MediaController.Current.BackgroundAudioTaskStarted.WaitOne(10000);
            if (result)
            {
                MediaController.Current.UpdatePlaybackList(MediaController.Current.SongList, false);
                //MediaController.Current.MessageReceivedFromBackground += Current_MessageReceivedFromBackground1;
            }
        }

        private void Current_PlaylistChanged(MediaController sender, PlaylistChangedEventArgs args)
        {
            DispatcherHelper.CheckBeginInvokeOnUI(() =>
            {
                Playlist.Clear();
                foreach (var item in args.NewList)
                {
                    Playlist.Add(item);
                }
            });
        }

        private void RemoveEventHandler()
        {
            WakeUpSpeechRecognizer.Instance.StateChanged -= WakeUpSpeech_StateChanged;
            WakeUpSpeechRecognizer.Instance.HypothesisGenerated -= WakeUpSpeechRecognizer_HypothesisGenerated;
            WakeUpSpeechRecognizer.Instance.ContinuousRecognitionSessionCompleted -= WakeUpContinuousRecognitionSessionCompleted;
            WakeUpSpeechRecognizer.Instance.ContinuousRecognitionSessionResultGenerated -= WakeUpSpeech_ResultGenerated;

            CommandSpeechRecognizer.Instance.StateChanged -= CommandSpeech_StateChanged;
            CommandSpeechRecognizer.Instance.HypothesisGenerated -= CommandSpeech_HypothesisGenerated;
            CommandSpeechRecognizer.Instance.ContinuousRecognitionSessionResultGenerated -= CommandSpeech_ResultGenerated;
        }

        private void AddEventHandler()
        {
            WakeUpSpeechRecognizer.Instance.StateChanged += WakeUpSpeech_StateChanged;
            WakeUpSpeechRecognizer.Instance.HypothesisGenerated += WakeUpSpeechRecognizer_HypothesisGenerated;
            WakeUpSpeechRecognizer.Instance.ContinuousRecognitionSessionCompleted += WakeUpContinuousRecognitionSessionCompleted;
            WakeUpSpeechRecognizer.Instance.ContinuousRecognitionSessionResultGenerated += WakeUpSpeech_ResultGenerated;

            CommandSpeechRecognizer.Instance.StateChanged += CommandSpeech_StateChanged;
            CommandSpeechRecognizer.Instance.HypothesisGenerated += CommandSpeech_HypothesisGenerated;
            CommandSpeechRecognizer.Instance.ContinuousRecognitionSessionResultGenerated += CommandSpeech_ResultGenerated;
        }

        private void WakeUpSpeech_ResultGenerated(SpeechContinuousRecognitionSession sender, SpeechContinuousRecognitionResultGeneratedEventArgs args)
        {
            string text = args.Result.Text;
            Log("Wake-up Result:" + text);
        }

        private async void WakeUpSpeechRecognizer_HypothesisGenerated(SpeechRecognizer sender, SpeechRecognitionHypothesisGeneratedEventArgs args)
        {
            string text = args.Hypothesis.Text;
            Log("Wake-up Hypothesis:" + text);

            if (text.Trim().ToLower() == WakeUpSpeechRecognizer.Instance.WakeUpString)
            {
                if (isPlaying)
                {
                    MediaController.Current.Pause();
                }
                if (!IsOnline)
                {
                    return;
                }
                if (_cancellationTokenSource != null)
                {
                    if (t.Status == TaskStatus.Running)
                    {
                        _cancellationTokenSource.Cancel();
                    }
                }
                Notify(NoticeType.Ready);
                await WakeUpSpeechRecognizer.StopAsync();
            }

            //string name = _speechResourceMap.GetValue("HelloSandy", _speechContext).ValueAsString.ToLower();

            //string formatText = text.Trim().ToLower();
            //if (formatText.Equals(name))
            //{
            //    if (isPlaying)
            //    {
            //        MediaController.Current.Pause();
            //    }

            //    //如果网络未链接，提示使用手机联网
            //    if (!IsOnline)
            //    {
            //        return;
            //    }

            //    if (_cancellationTokenSource != null)
            //    {
            //        if (t.Status == TaskStatus.Running)
            //        {
            //            _cancellationTokenSource.Cancel();
            //        }
            //        //_cancellationTokenSource.Cancel();
            //    }
            //    //NotifyWithSound(this, new NotifyWithSoundEventArgs() { NoticeType = NoticeType.Ready });
            //    Notify(NoticeType.Ready);
            //    try
            //    {
            //        await _speechRecognizerForWakeUp.ContinuousRecognitionSession.CancelAsync();
            //    }
            //    catch { }
            //}

            //await UpdateText("Hypothesis:" + args.Hypothesis.Text);
        }

        private async void WakeUpContinuousRecognitionSessionCompleted(SpeechContinuousRecognitionSession sender, SpeechContinuousRecognitionCompletedEventArgs args)
        {
            Log("Recognition Session Ended:" + args.Status);
            if (args.Status == SpeechRecognitionResultStatus.Success)
            {
                if (CommandSpeechRecognizer.Instance.State != SpeechRecognizerState.Idle)
                {
                    try
                    {
                        //await _speechRecognizerForCommand.ContinuousRecognitionSession.CancelAsync();
                        await CommandSpeechRecognizer.StopAsync();
                    }
                    catch
                    { }
                }

                catchedSomething = false;
                SpeechAndMusicResetEvent.Reset();
                //rec.ContinuousRecognitionSession.AutoStopSilenceTimeout = new TimeSpan(200);
                try
                {
                    //await _speechRecognizerForCommand.ContinuousRecognitionSession.StartAsync();
                    await CommandSpeechRecognizer.StartAsync();
                }
                catch { }
                //var result = await rec.RecognizeAsync();


                //await App.MainFrame.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, async () =>
                //{
                //    await new MessageDialog(result.Text).ShowAsync();
                //});

                //try
                //{
                //    //Get intent from utterance
                //    var responseModel = await IntelligentServiceHelper.GetCortanaModelResponse(result.Text);

                //    ExecuteSpeechCommand(responseModel, result.Text);
                //}
                //catch (Exception ex)
                //{
                //    Log(ex.Message);
                //}
            }
            else
            {
                if (WakeUpSpeechRecognizer.Instance.State == SpeechRecognizerState.Idle)
                {
                    await WakeUpSpeechRecognizer.StartAsync();
                }
            }

            //var result = DualSpeeckRecResetEvent.WaitOne();
            //await sender.StartAsync(SpeechContinuousRecognitionMode.Default);
        }

        private void WakeUpSpeech_StateChanged(SpeechRecognizer sender, SpeechRecognizerStateChangedEventArgs args)
        {
            string status = args.State.ToString();
            UpdateStatus(status, false);
        }

        private async void CommandSpeech_ResultGenerated(SpeechContinuousRecognitionSession sender, SpeechContinuousRecognitionResultGeneratedEventArgs args)
        {
            ThinkResetEvent.Set();
            //NotifyWithSound(this, new NotifyWithSoundEventArgs() { NoticeType = NoticeType.Thinking });
            catchedSomething = true;
            var result = args.Result;
            Log("识别结果:" + result.Text);
            try
            {
                await CommandSpeechRecognizer.StopAsync();
            }
            catch (Exception ex)
            {
                Log(CommandSpeechRecognizer.Instance.State.ToString());
            }


            try
            {
                if (_cancellationTokenSource != null)
                {
                    _cancellationTokenSource.Dispose();
                    _cancellationTokenSource = null;
                }

                _cancellationTokenSource = new CancellationTokenSource();

                t = Task.Factory.StartNew(() =>
                {
                    try
                    {
                        Task<QueryResponse> task = IntelligentServiceHelper.GetCortanaModelResponse(result.Text);
                        task.Wait();
                        var response = task.Result;

                        ExecuteSpeechCommand(response, result.Text);
                    }
                    catch (Exception ex)
                    {
                        Log(ex.ToString() + ":" + ex.Message);
                    }

                }, _cancellationTokenSource.Token);

                Log("任务状态：" + t.Status.ToString());

            }
            catch (Exception ex)
            {
                Log(ex.Message);
            }
            finally
            {
                DualSpeeckRecResetEvent.Set();
            }
        }

        private void CommandSpeech_HypothesisGenerated(SpeechRecognizer sender, SpeechRecognitionHypothesisGeneratedEventArgs args)
        {
            UpdateText(args.Hypothesis.Text);
            Log("Command Hypothesis:" + args.Hypothesis.Text);
        }

        private async void CommandSpeech_StateChanged(SpeechRecognizer sender, SpeechRecognizerStateChangedEventArgs args)
        {
            Log("Command Recognizer Status:" + args.State.ToString());
            if (args.State == SpeechRecognizerState.Idle)
            {
                Notify(NoticeType.Thinking);

                await Task.Factory.StartNew(async () =>
                {
                    bool result = ThinkResetEvent.WaitOne(10000);
                    if (!result)
                    {
                        string text = "";
                        await App.MainFrame.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                        {
                            text = AppResources.cannot_catch_what_you_said;
                        });
                        Notify(NoticeType.Speech, text);

                        if (isPlaying)
                        {
                            Log("等待");
                            if (SpeechAndMusicResetEvent.WaitOne(5000))
                            {
                                Log("释放");
                                MediaController.Current.CurrentPlayer.Play();
                            }
                        }
                    }

                    try
                    {
                        await WakeUpSpeechRecognizer.StartAsync();
                    }
                    catch (Exception)
                    {
                        throw;
                    }
                });

            }
        }

        /// <summary>
        /// 初始化一些东西
        /// </summary>
        private async void Initialize()
        {
            bool audioCapturePermission = await AudioCapturePermission.RequestMicrophonePermission();

            if (audioCapturePermission)
            {
                var item = await _mainDataService.GetMainData();
                SpeechStatus = item.NoneStatus;

                _speechResourceMap = ResourceManager.Current.MainResourceMap.GetSubtree("LocalizationSpeechResources");

                Language speechLanguage = SpeechRecognizer.SystemSpeechLanguage;

                _speechContext = ResourceContext.GetForCurrentView();
                _speechContext.Languages = new string[] { speechLanguage.LanguageTag };

                Languages = new ObservableCollection<Language>();
                int index = 0;
                foreach (var language in SpeechRecognizer.SupportedGrammarLanguages)
                {
                    Languages.Add(language);
                    if (language.LanguageTag == speechLanguage.LanguageTag)
                    {
                        SelectedLanguageIndex = index;
                    }
                    index++;
                }

                await Task.Factory.StartNew(async () =>
                {
                    //AddEventHandler();
                    //var ignore = Task.Delay(1000);
                    //ignore.Wait();
                    if (!IsOnline)
                    {
                        await App.MainFrame.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                        {
                            Notify(NoticeType.Speech, AppResources.network_failed);
                        });

                        InitializeOfflineRecognizer();

                        SpeechAndMusicResetEvent.WaitOne(20000);
                    }
                    else
                    {
                        await App.MainFrame.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                        {
                            Notify(NoticeType.Speech, AppResources.welcome_back);
                        });


                        SpeechAndMusicResetEvent.WaitOne(2000);
                        await InitializeRecognizer();
                    }
                });

            }
            else
            {
                UpdateStatus(AppResources.request_audio_failed, true);
            }
        }




        private async void InitializeOfflineRecognizer()
        {
            _speechRecognizerForOffline = new SpeechRecognizer(Languages[SelectedLanguageIndex]);
            _speechRecognizerForOffline.StateChanged += _speechRecognizerForOffline_StateChanged;
            _speechRecognizerForOffline.Constraints.Add(
                new SpeechRecognitionListConstraint(
                    new List<string>()
                    {
                        _speechResourceMap.GetValue("Yes", _speechContext).ValueAsString
                    }, "yes"));
            _speechRecognizerForOffline.Constraints.Add(
                new SpeechRecognitionListConstraint(
                    new List<string>()
                    {
                        _speechResourceMap.GetValue("No", _speechContext).ValueAsString
                    }, "no"));

            SpeechRecognitionCompilationResult compilationResult = await _speechRecognizerForOffline.CompileConstraintsAsync();
            if (compilationResult.Status != SpeechRecognitionResultStatus.Success)
            {
                Log("Compile offline recognizer's constraint failed! Status: " + compilationResult.Status.ToString());
            }
            else
            {
                _speechRecognizerForOffline.HypothesisGenerated += _speechRecognizerForOffline_HypothesisGenerated;
            }
        }

        private async void _speechRecognizerForOffline_HypothesisGenerated(SpeechRecognizer sender, SpeechRecognitionHypothesisGeneratedEventArgs args)
        {
            Log("Offline Hypothesis: " + args.Hypothesis.Text);
            string text = args.Hypothesis.Text;
            string yes = _speechResourceMap.GetValue("Yes", _speechContext).ValueAsString;
            string no = _speechResourceMap.GetValue("No", _speechContext).ValueAsString;
            if (text.Equals(yes))
            {
                Notify(NoticeType.Speech, "进入联网模式");
                bool result = await WlanConnector.TryConnectWlan();
                if (result)
                {
                    Notify(NoticeType.Speech, "Connection Succeed");
                }
                else
                {
                    Notify(NoticeType.Speech, "Connection Failed");
                }
            }
            else if (text.Equals(no))
            {
                Notify(NoticeType.Speech, "进入离线模式");
            }

        }

        private void _speechRecognizerForOffline_StateChanged(SpeechRecognizer sender, SpeechRecognizerStateChangedEventArgs args)
        {
            Log("Offline Rec Status:" + args.State.ToString());
        }


        /// <summary>
        /// 初始化SpeechRecognizer
        /// </summary>
        /// <param name="recognizerLanguage"></param>
        /// <returns></returns>
        private async Task InitializeRecognizer()
        {
            if (isInitializing) return;

            isInitializing = true;

            try
            {
                AddEventHandler();
                await WakeUpSpeechRecognizer.StartAsync();
            }
            catch (Exception ex)
            {
                if ((uint)ex.HResult == HResultRecognizerNotFound)
                {
                    UpdateStatus("Speech Language pack is not installed!", true);
                }
                else
                {
                    UpdateStatus("Exception:" + ex.Message, true);
                }
            }
            finally
            {
                isInitializing = false;
            }
        }

        private async Task DisposeRecognizer()
        {
            await CommandSpeechRecognizer.StopAsync();
            await WakeUpSpeechRecognizer.StopAsync();
            RemoveEventHandler();
            CommandSpeechRecognizer.Dispose();
            WakeUpSpeechRecognizer.Dispose();
        }

        /// <summary>
        /// 执行playmusic命令
        /// </summary>
        /// <param name="response"></param>
        private async void PlayMusic(QueryResponse response)
        {
            if (response.entities?.Count() == 0)
            {
                bool result = MediaController.Current.Play();
                if (!result)
                {
                    await App.MainFrame.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                    {
                        //NotifyWithSound(this, new NotifyWithSoundEventArgs()
                        //{
                        //    NoticeType = NoticeType.Speech,
                        //    Text = AppResources.cannot_find_any_music
                        //});
                        Notify(NoticeType.Speech, AppResources.cannot_find_any_music);
                    });
                }
                else
                {
                    isPlaying = true;
                    NotifyWithSound(this, new NotifyWithSoundEventArgs() { NoticeType = NoticeType.Completed });
                }
            }
            else
            {
                bool result = MediaController.Current.Play(response.entities);
                await App.MainFrame.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                {
                    if (!result)
                    {
                        //NotifyWithSound(this, new NotifyWithSoundEventArgs()
                        //{
                        //    NoticeType = NoticeType.Speech,
                        //    Text = AppResources.cannot_find_any_music_requred
                        //});
                        Notify(NoticeType.Speech, AppResources.cannot_find_any_music_requred);
                    }
                    else
                    {
                        isPlaying = true;
                        //NotifyWithSound(this, new NotifyWithSoundEventArgs() { NoticeType = NoticeType.Completed });
                        Notify(NoticeType.Completed);
                    }
                });

            }
        }

        /// <summary>
        /// 执行pause命令
        /// </summary>
        private void Pause()
        {
            MediaController.Current.Pause();
            isPlaying = false;
            //NotifyWithSound(this, new NotifyWithSoundEventArgs() { NoticeType = NoticeType.Completed });
            Notify(NoticeType.Completed);
        }

        /// <summary>
        /// 执行上一曲命令
        /// </summary>
        private void SkipBack()
        {
            MediaController.Current.Previous();
            //NotifyWithSound(this, new NotifyWithSoundEventArgs() { NoticeType = NoticeType.Completed });
            Notify(NoticeType.Completed);
        }

        /// <summary>
        /// 执行下一曲命令
        /// </summary>
        private void SkipForward()
        {
            MediaController.Current.Next();
            //NotifyWithSound(this, new NotifyWithSoundEventArgs() { NoticeType = NoticeType.Completed });
            Notify(NoticeType.Completed);
        }

        /// <summary>
        /// 设置闹钟
        /// </summary>
        /// <param name="queryResponse"></param>
        private void SetAlarm(QueryResponse queryResponse)
        {
        }

        /// <summary>
        /// 判定语音命令类型并执行
        /// </summary>
        /// <param name="response"></param>
        /// <param name="utterance"></param>
        private void ExecuteSpeechCommand(QueryResponse queryResponse, string utterance)
        {
            try
            {

                if (queryResponse.topScoringIntent != null)
                {
                    Intent intent = queryResponse.topScoringIntent;
                    var intentType = IntelligentServiceHelper.GetIntentType(intent);
                    Log("意图：" + intentType.ToString());

                    bool waitResult = true;
                    if (!_cancellationTokenSource.IsCancellationRequested)
                    {
                        switch (intentType)
                        {
                            case IntentType.SwitchLanguage:
                                Log("Intent:" + IntentType.SwitchLanguage.ToString());
                                //ChangeLanguage(queryResponse);
                                break;
                            case IntentType.SetAlarm:
                                //To-Do: 设置闹钟
                                SetAlarm(queryResponse);
                                waitResult = SpeechAndMusicResetEvent.WaitOne(8000);
                                break;
                            case IntentType.PlayMusic:
                                //To-Do: 播放音乐
                                PlayMusic(queryResponse);
                                waitResult = SpeechAndMusicResetEvent.WaitOne(8000);
                                break;

                            case IntentType.Pause:
                                //To-Do: 媒体控制(暂停，继续，上一曲，下一曲等)
                                Pause();
                                waitResult = SpeechAndMusicResetEvent.WaitOne(8000);
                                break;

                            case IntentType.Goodbye:
                                //To-Do: 音箱休眠
                                Log("意图:" + intent.intent);
                                break;

                            case IntentType.AddToFavorite:
                                //To-Do: 收藏当前歌曲
                                Log("意图:" + intent.intent);
                                break;

                            case IntentType.SkipForward:
                                //To-Do: 下一首 
                                Log("意图:" + intent.intent);
                                SkipForward();
                                waitResult = SpeechAndMusicResetEvent.WaitOne(8000);
                                break;

                            case IntentType.SkipBack:
                                //To-Do: 上一首
                                Log("意图:" + intent.intent);
                                SkipBack();
                                waitResult = SpeechAndMusicResetEvent.WaitOne(8000);
                                break;

                            case IntentType.Resume:
                                //To-Do: 恢复播放
                                Log("意图:" + intent.intent);
                                break;

                            case IntentType.CheckWeather:
                            case IntentType.QuestionWeather:
                                //To-Do: 查询天气
                                Log("意图:" + intent.intent);
                                StringBuilder sb = new StringBuilder();
                                //string result = await WeatherHelper.CheckWeather(queryResponse.entities);
                                Task<string> task = WeatherHelper.CheckWeather(queryResponse.entities);
                                //task.Start();
                                task.Wait();
                                string result = task.Result;
                                if (string.IsNullOrEmpty(result))
                                {
                                    result = "Sorry!";
                                }
                                //NotifyWithSound(this, new NotifyWithSoundEventArgs() { NoticeType = NoticeType.Speech, Text = result });
                                if (!_cancellationTokenSource.IsCancellationRequested)
                                {
                                    Notify(NoticeType.Speech, result);
                                    waitResult = SpeechAndMusicResetEvent.WaitOne(20000);
                                }
                                break;

                            case IntentType.None:
                                //To-Do: none
                                if (isPrebuiltApp)
                                {
                                    Log("意图:" + intent.intent);
                                    //var myAppResponse = await IntelligentServiceHelper.GetResponse(utterance);
                                    Task<QueryResponse> task1 = IntelligentServiceHelper.GetResponse(utterance);
                                    //task1.Start();
                                    task1.Wait();
                                    var myAppResponse = task1.Result;
                                    isPrebuiltApp = false;
                                    ExecuteSpeechCommand(myAppResponse, utterance);
                                    isPrebuiltApp = true;
                                    return;
                                }
                                else
                                {
                                    Log("使用图灵机器人回复");
                                    //var tuling = await TulingRobotClient.CurrentClient.GetResponse(utterance);
                                    Task<Response> task2 = TulingRobotClient.CurrentClient.GetResponse(utterance);
                                    //task2.Start();
                                    task2.Wait();
                                    var tuling = task2.Result;
                                    if (!_cancellationTokenSource.IsCancellationRequested)
                                    {
                                        SpeakTuling(tuling);
                                        waitResult = SpeechAndMusicResetEvent.WaitOne(80000);
                                    }
                                    //string text = "";
                                    //await App.MainFrame.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                                    //{
                                    //    text = AppResources.cannot_catch_what_you_want;
                                    //});
                                    //NotifyWithSound(this, new NotifyWithSoundEventArgs() { NoticeType = NoticeType.Speech, Text = text });
                                    break;
                                }
                        }
                    }
                    else
                    {
                        waitResult = true;
                    }

                    if (waitResult)
                    {
                        if (isPlaying && !_cancellationTokenSource.IsCancellationRequested)
                        {
                            MediaController.Current.CurrentPlayer.Play();
                        }
                    }
                    else
                    {
                        //throw new Exception("cannot complete task in expect time");
                        Log("cannot complete task in expect time");
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(utterance, ex);
            }
        }

        private async void ChangeLanguage(QueryResponse queryResponse)
        {
            await this.DisposeRecognizer();
            var lanEntity = queryResponse.entities.FirstOrDefault(p => p.type == "sounder.settings.language");
            if (lanEntity != null)
            {
                Language lan = null;
                if (lanEntity.entity.ToLower().Equals(AppResources.English))
                {
                    lan = new Language("en-us");
                }
                else if (lanEntity.entity.ToLower().Equals(AppResources.Chinese))
                {
                    lan = new Language("zh-ch");
                }
                if (lan != null && lan.NativeName != _languages[_selectedLanguageIndex].NativeName)
                {
                    DispatcherHelper.CheckBeginInvokeOnUI(() =>
                    {
                        SelectedLanguageIndex = Math.Abs(SelectedLanguageIndex - 1);
                    });
                }
                AppSettings.SetValue(AppSettingsConstants.LANGUAGE_SETTING, _languages[_selectedLanguageIndex].LanguageTag);
                await InitializeRecognizer();
            }
        }



        /// <summary>
        /// 图灵机器人回复
        /// </summary>
        /// <param name="tuling"></param>
        private async void SpeakTuling(Response tuling)
        {
            switch (tuling.code)
            {
                case "100000":
                    string text = tuling.text.Replace("<br>", "").Trim();
                    if (tuling.text.Contains("\n"))
                    {
                        var texts = text.Split('\n');
                        foreach (var item in texts)
                        {
                            if (!item.Contains("运单流程"))
                            {
                                text = item;
                                break;
                            }
                        }
                    }
                    //NotifyWithSound(this, new NotifyWithSoundEventArgs() { NoticeType = NoticeType.Speech, Text = text });
                    Notify(NoticeType.Speech, text);
                    //if (tuling.text.Contains("快递单号是多少"))
                    //{
                    //    try
                    //    {
                    //        await _speechRecognizer.ContinuousRecognitionSession.CancelAsync();
                    //        await rec.ContinuousRecognitionSession.StartAsync();
                    //    }
                    //    catch(Exception ex)
                    //    {
                    //        throw;
                    //    }
                    //}
                    break;
                case "200000":
                case "302000":
                case "308000":
                    //NotifyWithSound(this, new NotifyWithSoundEventArgs() { NoticeType = NoticeType.Speech, Text = tuling.text + "，发送到您手机上了，请注意查收哦！" });
                    Notify(NoticeType.Speech, tuling.text + ", 发送到您手机上了,请注意查收");
                    break;
                default:
                    await App.MainFrame.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                    {
                        //NotifyWithSound(this, new NotifyWithSoundEventArgs() { NoticeType = NoticeType.Speech, Text = AppResources.cannot_catch_what_you_want });
                        Notify(NoticeType.Speech, AppResources.cannot_catch_what_you_want);
                    });
                    break;

            }
        }

        /// <summary>
        /// 更新UI上的状态信息
        /// </summary>
        /// <param name="text"></param>
        /// <param name="isWarning"></param>
        /// <returns></returns>
        public void UpdateStatus(string text, bool isWarning = false)
        {
            DispatcherHelper.CheckBeginInvokeOnUI(() =>
            {

                SpeechStatus = text;
                if (isWarning)
                    StatusColor = new SolidColorBrush(Colors.Red);
                else
                    StatusColor = new SolidColorBrush(Colors.Green);
            });

        }

        /// <summary>
        /// 在UI上更新语音识别结果
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        private void UpdateText(string text)
        {
            DispatcherHelper.CheckBeginInvokeOnUI(() =>
            {
                Text = text;
            });
        }

        /// <summary>
        /// 播放提醒声音或语音
        /// </summary>
        /// <param name="type"></param>
        /// <param name="text"></param>
        private void Notify(NoticeType type, string text = null)
        {
            NotifyWithSound(this, new NotifyWithSoundEventArgs() { NoticeType = type, Text = text });
        }

        #endregion

        #region Event Handlers

        private async void _speechRecognizerForCommand_StateChanged(SpeechRecognizer sender, SpeechRecognizerStateChangedEventArgs args)
        {
            await App.MainFrame.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                CommandRecStatus = args.State.ToString();
            });
        }

        /// <summary>
        /// 在UI上实时更新识别到的命令语句
        /// </summary>
        /// <param name = "sender" ></ param >
        /// < param name="args"></param>
        //private void Rec_HypothesisGenerated(SpeechRecognizer sender, SpeechRecognitionHypothesisGeneratedEventArgs args)
        //{
        //    UpdateText(args.Hypothesis.Text);
        //    Log("Command Hypothesis:" + args.Hypothesis.Text);
        //}

        /// <summary>
        /// 识别唤醒语句
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void _speechRecognizer_HypothesisGenerated(SpeechRecognizer sender, SpeechRecognitionHypothesisGeneratedEventArgs args)
        {

            string text = args.Hypothesis.Text;
            Log("Wake-up Hypothesis:" + text);

            string name = _speechResourceMap.GetValue("HelloSandy", _speechContext).ValueAsString.ToLower();

            string formatText = text.Trim().ToLower();
            if (formatText.Equals(name))
            {
                if (isPlaying)
                {
                    MediaController.Current.Pause();
                }

                //如果网络未链接，提示使用手机联网
                if (!IsOnline)
                {
                    return;
                }

                if (_cancellationTokenSource != null)
                {
                    if (t.Status == TaskStatus.Running)
                    {
                        _cancellationTokenSource.Cancel();
                    }
                    //_cancellationTokenSource.Cancel();
                }
                //NotifyWithSound(this, new NotifyWithSoundEventArgs() { NoticeType = NoticeType.Ready });
                Notify(NoticeType.Ready);
                try
                {
                    //await _speechRecognizerForWakeUp.ContinuousRecognitionSession.CancelAsync();
                }
                catch { }
            }

            UpdateText("Hypothesis:" + args.Hypothesis.Text);
        }

        /// <summary>
        /// 接收后台任务消息的Event Handler
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Current_MessageReceivedFromBackground(object sender, Windows.Media.Playback.MediaPlayerDataReceivedEventArgs e)
        {
            //接收到后台任务启动的消息后，释放等待事件，以向后台发送更新播放列表的消息
            BackgroundAudioTaskStartedMessage backgroundAudioTaskStartedMessage;
            if (MessageService.TryParseMessage(e.Data, out backgroundAudioTaskStartedMessage))
            {
                MediaController.Current.MessageReceivedFromBackground -= Current_MessageReceivedFromBackground;
                MediaController.Current.BackgroundAudioTaskStarted.Set();
                return;
            }

        }

        //private async void ContinuousRecognitionSession_ResultGenerated(SpeechContinuousRecognitionSession sender, SpeechContinuousRecognitionResultGeneratedEventArgs args)
        //{


        //    //string tag = "unknown";

        //    //if (args.Result.Confidence == SpeechRecognitionConfidence.High ||
        //    //    args.Result.Confidence == SpeechRecognitionConfidence.Medium)
        //    //{
        //    //    tag = args.Result.Constraint.Tag;
        //    //    if (tag == "Sandy")
        //    //    {
        //    //        MediaHelper.PlaySound(MediaHelper.SoundType.Ready);
        //    //        await _speechRecognizer.ContinuousRecognitionSession.CancelAsync();


        //    //    }
        //    //}
        //    //await UpdateText("Result:" + tag + "(Result Text:" + args.Result.Text + ", Confidence:" + args.Result.Confidence.ToString() + ")");

        //}

        /// <summary>
        /// 语音唤醒后，停止语音唤醒器，启动语音命令识别器
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private async void ContinuousRecognitionSession_Completed(SpeechContinuousRecognitionSession sender, SpeechContinuousRecognitionCompletedEventArgs args)
        {
            Log("Recognition Session Ended:" + args.Status);
            if (args.Status == SpeechRecognitionResultStatus.Success)
            {
                if (CommandSpeechRecognizer.Instance.State != SpeechRecognizerState.Idle)
                {
                    try
                    {
                        //await _speechRecognizerForCommand.ContinuousRecognitionSession.CancelAsync();
                        await CommandSpeechRecognizer.StopAsync();
                    }
                    catch
                    { }
                }

                catchedSomething = false;
                SpeechAndMusicResetEvent.Reset();
                //rec.ContinuousRecognitionSession.AutoStopSilenceTimeout = new TimeSpan(200);

                //var result = await rec.RecognizeAsync();


                //await App.MainFrame.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, async () =>
                //{
                //    await new MessageDialog(result.Text).ShowAsync();
                //});

                //try
                //{
                //    //Get intent from utterance
                //    var responseModel = await IntelligentServiceHelper.GetCortanaModelResponse(result.Text);

                //    ExecuteSpeechCommand(responseModel, result.Text);
                //}
                //catch (Exception ex)
                //{
                //    Log(ex.Message);
                //}
            }

            //var result = DualSpeeckRecResetEvent.WaitOne();
            //await sender.StartAsync(SpeechContinuousRecognitionMode.Default);
            try
            {
                //await _speechRecognizerForCommand.ContinuousRecognitionSession.StartAsync();
                await CommandSpeechRecognizer.StartAsync();
            }
            catch { }
        }

        Task t;
        /// <summary>
        /// 识别到语音命令后，执行语音命令并恢复语音唤醒器
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private async void ContinuousRecognitionSession_ResultGenerated1(SpeechContinuousRecognitionSession sender, SpeechContinuousRecognitionResultGeneratedEventArgs args)
        {
            ThinkResetEvent.Set();
            //NotifyWithSound(this, new NotifyWithSoundEventArgs() { NoticeType = NoticeType.Thinking });
            catchedSomething = true;
            var result = args.Result;
            Log("识别结果:" + result.Text);
            try
            {
                //await _speechRecognizerForCommand.ContinuousRecognitionSession.StopAsync();
                await CommandSpeechRecognizer.StopAsync();
            }
            catch (Exception ex)
            {
                Log(CommandSpeechRecognizer.Instance.State.ToString());
            }

            //await App.MainFrame.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, async () =>
            //{
            //    await new MessageDialog(result.Text).ShowAsync();
            //});

            try
            {
                if (_cancellationTokenSource != null)
                {
                    _cancellationTokenSource.Dispose();
                    _cancellationTokenSource = null;
                }

                _cancellationTokenSource = new CancellationTokenSource();


                t = Task.Factory.StartNew(() =>
                {
                    try
                    {
                        //Get intent from utterance
                        //var response = await IntelligentServiceHelper.GetCortanaModelResponse(result.Text);
                        Task<QueryResponse> task = IntelligentServiceHelper.GetCortanaModelResponse(result.Text);
                        //task.Start();
                        task.Wait();
                        var response = task.Result;

                        ExecuteSpeechCommand(response, result.Text);
                    }
                    catch (Exception ex)
                    {

                    }

                }, _cancellationTokenSource.Token);

                Log("任务状态：" + t.Status.ToString());

            }
            catch (Exception ex)
            {
                Log(ex.Message);
            }
            finally
            {
                DualSpeeckRecResetEvent.Set();
            }
        }

        ///// <summary>
        ///// 
        ///// </summary>
        ///// <param name="sender"></param>
        ///// <param name="args"></param>
        //private async void Rec_StateChanged(SpeechRecognizer sender, SpeechRecognizerStateChangedEventArgs args)
        //{
        //    Log("Command Recognizer Status:" + args.State.ToString());
        //    if (args.State == SpeechRecognizerState.Idle)
        //    {
        //        //NotifyWithSound(this, new NotifyWithSoundEventArgs() { NoticeType = NoticeType.Thinking });
        //        Notify(NoticeType.Thinking);

        //        await Task.Factory.StartNew(async () =>
        //        {
        //            bool result = ThinkResetEvent.WaitOne(10000);
        //            if (!result)
        //            {
        //                string text = "";
        //                await App.MainFrame.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
        //                {
        //                    text = AppResources.cannot_catch_what_you_said;
        //                });
        //                //NotifyWithSound(this, new NotifyWithSoundEventArgs() { NoticeType = NoticeType.Speech, Text = text });
        //                Notify(NoticeType.Speech, text);



        //                //if (rec.State != SpeechRecognizerState.Idle)
        //                //{
        //                //    await rec.ContinuousRecognitionSession.CancelAsync();
        //                //}
        //                if (isPlaying)
        //                {
        //                    Log("等待");
        //                    if (SpeechAndMusicResetEvent.WaitOne(5000))
        //                    {
        //                        Log("释放");
        //                        MediaController.Current.CurrentPlayer.Play();
        //                    }
        //                }
        //            }

        //            try
        //            {
        //                //await _speechRecognizerForWakeUp.ContinuousRecognitionSession.StartAsync();
        //                await WakeUpSpeechRecognizer.StartAsync();
        //            }
        //            catch (Exception)
        //            {

        //                throw;
        //            }
        //        });

        //    }

        //}

        /// <summary>
        /// 在UI上实时更新语音唤醒器状态
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void _speechRecognizer_StateChanged(SpeechRecognizer sender, SpeechRecognizerStateChangedEventArgs args)
        {
            string status = args.State.ToString();
            UpdateStatus(status, false);
        }

        #endregion

        #region Construct

        public MainViewModel(IMainDataService mainDataService, INavigationService navigationService)
        {
            App.Current.UnhandledException += Current_UnhandledException;
            App.Current.Suspending += Current_Suspending;
            _mainDataService = mainDataService;
            this._navigationService = navigationService;

            ApplicationLanguages.PrimaryLanguageOverride = AppSettingsConstants.CurrentLanguage;
            InitializeLocalMedia();
            Initialize();
        }

        private async void Current_Suspending(object sender, Windows.ApplicationModel.SuspendingEventArgs e)
        {
            StorageFile file = await ApplicationData.Current.LocalFolder.CreateFileAsync("log", CreationCollisionOption.GenerateUniqueName);
            using (var stream = await file.OpenAsync(FileAccessMode.ReadWrite))
            {
                StreamWriter writer = new StreamWriter(stream.AsStream());
                writer.Write(LogStr);
                LogStr = "";
                writer.Flush();
            }
            UDPServer.Current?.SaveClientList();
        }

        private async void Current_UnhandledException(object sender, Windows.UI.Xaml.UnhandledExceptionEventArgs e)
        {

        }

        #endregion
    }
}
