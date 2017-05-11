using BackgroundAudioProtocol.BackgroundAudioSettings;
using BackgroundAudioProtocol.Messages;
using BackgroundAudioProtocol.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using UWPHelpers;
using Windows.ApplicationModel.Background;
using Windows.Foundation;
using Windows.Media;
using Windows.Media.Core;
using Windows.Media.Playback;
using Windows.Storage;
using Windows.Storage.Streams;
using System.Linq;

namespace BackgroundAudioTask
{
    public sealed class BackgroundAudioTask : IBackgroundTask
    {
        #region Private fields

        private bool isRemoteControl = false;

        /// <summary>
        /// 系统音频播放控制器(在Mobile上，按音量键弹出)
        /// </summary>
        private SystemMediaTransportControls smtc;

        /// <summary>
        /// 前台应用程序状态
        /// </summary>
        private AppState foregroundAppState = AppState.Unknown;

        private BackgroundTaskDeferral deferral;

        /// <summary>
        /// 同步后台任务启动的等待时间
        /// </summary>
        private ManualResetEvent backgroundTaskStarted = new ManualResetEvent(false);

        /// <summary>
        /// 后台播放列表
        /// </summary>
        private MediaPlaybackList playbackList;

        /// <summary>
        /// 是否第一次执行播放命令
        /// </summary>
        private bool isFirstTimeToStart = false;

        /// <summary>
        /// 接收前台消息的线程对象锁
        /// </summary>
        private object o;

        private List<SongModel> playlist;

        private int currentIndex = -1;

        private delegate void CurrentPlayItemChangedEventHandler(List<SongModel> playlist, CurrentMediaPlayItemChangedEventArgs args);
        private event CurrentPlayItemChangedEventHandler CurrentPlayItemChanged;

        /// <summary>
        /// 表示当前后台音频是否正在播放
        /// </summary>
        private bool isPlaying
        {
            get
            {
                if (BackgroundMediaPlayer.Current.CurrentState == MediaPlayerState.Playing)
                {
                    return true;
                }
                return false;
            }
        }

        #endregion


        /// <summary>
        /// 后台任务的入口方法
        /// </summary>
        /// <param name="taskInstance">表示当前任务的实例</param>
        public void Run(IBackgroundTaskInstance taskInstance)
        {
            //初始化后台消息处理的线程锁
            o = new object();

            //初始化系统音频控制器
            smtc = BackgroundMediaPlayer.Current.SystemMediaTransportControls;
            smtc.ButtonPressed += Smtc_ButtonPressed;
            smtc.PropertyChanged += Smtc_PropertyChanged;
            smtc.IsEnabled = true;
            smtc.IsNextEnabled = true;
            smtc.IsPreviousEnabled = true;
            smtc.IsPlayEnabled = true;
            smtc.IsPauseEnabled = true;

            //获取前台APP状态
            var appState = BackgroundAudioSettingsHelper.GetValue(BackgroundAudioSettingsConstants.APP_STATE);

            if (appState == null)
            {
                foregroundAppState = AppState.Unknown;
            }
            else
            {
                foregroundAppState = EnumHelper.Parse<AppState>(appState.ToString());
            }

            //注册后台播放器的状态改变事件
            BackgroundMediaPlayer.Current.CurrentStateChanged += Current_CurrentStateChanged;

            //注册后台播放器发生错误时的事件
            BackgroundMediaPlayer.Current.MediaFailed += Current_MediaFailed;
            BackgroundMediaPlayer.Current.MediaEnded += Current_MediaEnded;

            //注册后台播放器接收前台消息事件
            BackgroundMediaPlayer.MessageReceivedFromForeground += BackgroundMediaPlayer_MessageReceivedFromForeground;

            //如果前台APP未处于挂起状态，发送后台音频启动消息给前台
            if (foregroundAppState != AppState.Suspended)
                MessageService.SendMessageToForeground(new BackgroundAudioTaskStartedMessage());

            //保存后台音频任务的启动状态
            BackgroundAudioSettingsHelper.SetValue(BackgroundAudioSettingsConstants.BACKGROUND_TASK_STATE, BackgroundTaskState.Running.ToString());

            deferral = taskInstance.GetDeferral();

            backgroundTaskStarted.Set();

            //注册后台任务结束事件
            taskInstance.Task.Completed += Task_Completed;

            //注册后台任务取消事件
            taskInstance.Canceled += TaskInstance_Canceled;

        }

        private void Current_MediaEnded(MediaPlayer sender, object args)
        {
            SkipToNext();
        }


        #region Event Handlers

        /// <summary>
        /// 后台播放器发生错误时的event handler
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args">包含错误类型、错误消息以及异常信息</param>
        private void Current_MediaFailed(MediaPlayer sender, MediaPlayerFailedEventArgs args)
        {
            Debug.WriteLine("Media error:" + args.Error + "---Error Info:" + args.ErrorMessage);
            SkipToNext();
        }

        /// <summary>
        /// 后台任务取消的event handler
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="reason">取消原因</param>
        private void TaskInstance_Canceled(IBackgroundTaskInstance sender, BackgroundTaskCancellationReason reason)
        {
            Debug.WriteLine("BackgroundAudioTask " + sender.Task.TaskId + " Cancel Requested...");

            try
            {
                backgroundTaskStarted.Reset();

                //保存前后台任务状态和歌曲播放信息
                BackgroundAudioSettingsHelper.SetValue(BackgroundAudioSettingsConstants.TRACK_INDEX, playbackList.CurrentItemIndex);
                BackgroundAudioSettingsHelper.SetValue(BackgroundAudioSettingsConstants.POSITION, BackgroundMediaPlayer.Current.Position.ToString());
                BackgroundAudioSettingsHelper.SetValue(BackgroundAudioSettingsConstants.BACKGROUND_TASK_STATE, BackgroundTaskState.Canceled.ToString());
                BackgroundAudioSettingsHelper.SetValue(BackgroundAudioSettingsConstants.APP_STATE, Enum.GetName(typeof(AppState), foregroundAppState));

                //清空播放列表
                if (playbackList != null)
                {
                    //playbackList.CurrentItemChanged -= PlaybackList_CurrentItemChanged;
                    playbackList = null;
                }

                //取消注册时间
                BackgroundMediaPlayer.MessageReceivedFromForeground -= BackgroundMediaPlayer_MessageReceivedFromForeground;
                smtc.ButtonPressed -= Smtc_ButtonPressed;
                smtc.PropertyChanged -= Smtc_PropertyChanged;

                //发送后台任务停止消息给前台
                MessageService.SendMessageToForeground(new BackgroundAudioTaskStopedMessage());
                //关闭后台播放器
                BackgroundMediaPlayer.Shutdown();

            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString() + "  " + ex.Message);
            }
            finally
            {
                deferral.Complete();
                Debug.WriteLine("BackgroundAudioTask Cancel Completed");
            }
        }

        /// <summary>
        /// 当前播放音乐改变的event handler
        /// 当前播放改变时，更新系统音乐控制器；如果前台应用是激活状态，发送track更新消息给前台，否则保存track信息
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args">包含改变前和改变后的播放列表项</param>
        //private void PlaybackList_CurrentItemChanged(MediaPlaybackList sender, CurrentMediaPlaybackItemChangedEventArgs args)
        //{
        //    var item = args.NewItem;

        //    Debug.WriteLine("Playback list changed current item---" + playbackList.CurrentItemIndex);

        //    UpdateUVCOnNewTrack(item);

        //    if (item != null)
        //    {
        //        //string message = item.Source.CustomProperties["filename"] as string;
        //        //MessageService.SendMessageToForeground(new TrackChangeMessage(message));
        //        MessageService.SendMessageToForeground(new TrackChangeMessage(currentIndex));
        //    }

        //    //int index = playbackList.Items.IndexOf(item);
        //    //if (foregroundAppState == AppState.Active)
        //    //{
        //    //    MessageService.SendMessageToForeground(new TrackChangeMessage(index));
        //    //    isRemoteControl = false;
        //    //}
        //    //else
        //    //    BackgroundAudioSettingsHelper.SetValue(BackgroundAudioSettingsConstants.TRACK_INDEX, index.ToString());
        //}

        /// <summary>
        /// 后台任务结束的event handler
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void Task_Completed(BackgroundTaskRegistration sender, BackgroundTaskCompletedEventArgs args)
        {
            Debug.WriteLine("BackgroundAudioTask " + sender.TaskId + " Completed");
            deferral.Complete();
        }

        /// <summary>
        /// 接收到前台消息的event handler
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BackgroundMediaPlayer_MessageReceivedFromForeground(object sender, MediaPlayerDataReceivedEventArgs e)
        {
            lock (o)
            {
                Debug.WriteLine("接收消息" + e.Data);
                AppSuspendingMessage appSuspendingMessage;
                if (MessageService.TryParseMessage(e.Data, out appSuspendingMessage))
                {
                    Debug.WriteLine("App Suspending");

                    foregroundAppState = AppState.Suspended;
                    BackgroundAudioSettingsHelper.SetValue(BackgroundAudioSettingsConstants.TRACK_INDEX, playbackList == null ? uint.MinValue : playbackList.CurrentItemIndex);
                    return;
                }

                AppResumedMessage appResumedMessage;
                if (MessageService.TryParseMessage(e.Data, out appResumedMessage))
                {
                    Debug.WriteLine("App Resumed");
                    foregroundAppState = AppState.Active;
                    return;
                }

                StartPlaybackMessage startPlaybackMessage;
                if (MessageService.TryParseMessage(e.Data, out startPlaybackMessage))
                {
                    Debug.WriteLine("Starting Playback");
                    StartPlayback();
                    return;
                }

                SkipNextMessage skipNextMessage;
                if (MessageService.TryParseMessage(e.Data, out skipNextMessage))
                {
                    Debug.WriteLine("Skip to Next");
                    SkipToNext();
                    return;
                }

                SkipPreviousMessage skipPreviousMessage;
                if (MessageService.TryParseMessage(e.Data, out skipPreviousMessage))
                {
                    Debug.WriteLine("Skip to Previous");
                    SkipToPrevious();
                    return;
                }

                UpdatePlaybackListMessage updatePlaybackListMessage;
                if (MessageService.TryParseMessage(e.Data, out updatePlaybackListMessage))
                {
                    Debug.WriteLine("Update Playback list");
                    //CreatePlaybackList(updatePlaybackListMessage.Songs);
                    CreatePlaylist(updatePlaybackListMessage.Songs);
                    isFirstTimeToStart = !updatePlaybackListMessage.IsResumed;
                    return;
                }

                TrackChangeMessage trackChangeMessage;
                if (MessageService.TryParseMessage(e.Data, out trackChangeMessage))
                {
                    Debug.WriteLine("---------------Track Change" + trackChangeMessage.TrackIndex + "当前track:" + currentIndex);

                    int index = trackChangeMessage.TrackIndex;
                    //smtc.PlaybackStatus = MediaPlaybackStatus.Changing;
                    try
                    {
                        //currentIndex = (int)index;
                        //var item = playbackList.MoveTo(index);
                        PlayIndex(index);
                    }
                    catch { }
                    //int index = playbackList.Items.ToList().FindIndex(p => (string)p.Source.CustomProperties["filename"] == trackChangeMessage.TrackFile);
                    //Debug.WriteLine("切换歌曲： " + index + "---" + trackChangeMessage.TrackFile);
                    //smtc.PlaybackStatus = MediaPlaybackStatus.Changing;
                    //playbackList.MoveTo((uint)index);
                    return;
                }

                Debug.WriteLine("处理消息结束");
            }
        }

        /// <summary>
        /// 后台播放器状态改变的event handler
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void Current_CurrentStateChanged(MediaPlayer sender, object args)
        {
            switch (sender.CurrentState)
            {
                case MediaPlayerState.Playing:
                    smtc.PlaybackStatus = MediaPlaybackStatus.Playing;
                    break;
                case MediaPlayerState.Paused:
                    smtc.PlaybackStatus = MediaPlaybackStatus.Paused;
                    break;
                case MediaPlayerState.Closed:
                    smtc.PlaybackStatus = MediaPlaybackStatus.Closed;
                    break;
            }
        }

        /// <summary>
        /// 系统音乐控制器属性改变的event handler
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void Smtc_PropertyChanged(SystemMediaTransportControls sender, SystemMediaTransportControlsPropertyChangedEventArgs args)
        {
        }

        /// <summary>
        /// 按下系统音乐控制器按钮的event handler
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void Smtc_ButtonPressed(SystemMediaTransportControls sender, SystemMediaTransportControlsButtonPressedEventArgs args)
        {
            switch (args.Button)
            {
                case SystemMediaTransportControlsButton.Play:
                    Debug.WriteLine("UVC play button pressed");

                    bool result = backgroundTaskStarted.WaitOne(5000);
                    if (!result)
                        throw new Exception("Background Tast didn't initialize in time");
                    StartPlayback();
                    break;
                case SystemMediaTransportControlsButton.Pause:
                    Debug.WriteLine("UVC pause button pressed");
                    try
                    {
                        BackgroundMediaPlayer.Current.Pause();
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex.ToString() + " " + ex.Message);
                    }
                    break;
                case SystemMediaTransportControlsButton.Next:
                    Debug.WriteLine("UVC next button pressed");
                    SkipToNext();
                    break;
                case SystemMediaTransportControlsButton.Previous:
                    Debug.WriteLine("UVC previous button pressed");
                    SkipToPrevious();
                    break;
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// 开始播放音乐。
        /// 如果是第一次启动播放，直接播放；如果不是第一次启动播放，需要跳转到上次保存的进度继续播放。
        /// </summary>
        //private void StartPlayback()
        //{
        //    try
        //    {
        //        BackgroundMediaPlayer.Current.Source = playbackList;

        //        var currentTrackIndex = BackgroundAudioSettingsHelper.GetValue(BackgroundAudioSettingsConstants.TRACK_INDEX);
        //        var currentTrackPosition = BackgroundAudioSettingsHelper.GetValue(BackgroundAudioSettingsConstants.POSITION);

        //        if (!isFirstTimeToStart)
        //        {
        //            uint index;
        //            if (currentTrackIndex != null && uint.TryParse(currentTrackIndex.ToString(), out index))
        //            {
        //                currentIndex = (int)index;
        //                if (currentTrackPosition == null)
        //                {
        //                    playbackList.MoveTo(index);
        //                    BackgroundMediaPlayer.Current.Play();
        //                }
        //                else
        //                {
        //                    TypedEventHandler<MediaPlaybackList, CurrentMediaPlaybackItemChangedEventArgs> handler = null;
        //                    handler =
        //                        (MediaPlaybackList list, CurrentMediaPlaybackItemChangedEventArgs args) =>
        //                        {
        //                            try
        //                            {
        //                                if (args.NewItem.Equals(playbackList.Items[(int)index]))
        //                                {
        //                                    playbackList.CurrentItemChanged -= handler;

        //                                    var position = TimeSpan.Parse((string)currentTrackPosition);
        //                                    BackgroundMediaPlayer.Current.Position = position;

        //                                    BackgroundMediaPlayer.Current.Play();
        //                                }
        //                            }
        //                            catch
        //                            {

        //                            }
        //                        };

        //                    playbackList.CurrentItemChanged += handler;
        //                    playbackList.MoveTo(index);
        //                }
        //            }
        //            else
        //            {
        //                BackgroundMediaPlayer.Current.Play();
        //            }

        //        }
        //        else
        //        {
        //            BackgroundMediaPlayer.Current.Play();
        //            isFirstTimeToStart = false;
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Debug.WriteLine(ex.ToString() + " " + ex.Message);
        //    }
        //}

        private void StartPlayback()
        {
            try
            {
                var currentTrackIndex = BackgroundAudioSettingsHelper.GetValue(BackgroundAudioSettingsConstants.TRACK_INDEX);
                var positionSetting = BackgroundAudioSettingsHelper.GetValue(BackgroundAudioSettingsConstants.POSITION);
                if (!isFirstTimeToStart)
                {
                    int index;
                    if (currentTrackIndex != null && int.TryParse(currentTrackIndex.ToString(), out index))
                    {
                        if (positionSetting == null)
                        {
                            PlayIndex(index);
                        }
                        else
                        {
                            var position = TimeSpan.Parse((string)positionSetting);
                            PlayIndexWithPosition(index, position);
                        }
                    }
                    else
                    {
                        PlayIndex(0);
                    }
                }
                else
                {
                    PlayIndex(0);
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// 下一曲
        /// </summary>
        //private void SkipToNext()
        //{
        //    try
        //    {
        //        smtc.PlaybackStatus = MediaPlaybackStatus.Changing;
        //        currentIndex++;
        //        playbackList.MoveNext();
        //    }
        //    catch { }
        //}
        private void SkipToNext()
        {
            try
            {
                var index = (currentIndex + 1) % playlist.Count;

                PlayIndex(index);
                CurrentPlayItemChanged?.Invoke(playlist, new CurrentMediaPlayItemChangedEventArgs() { NewItem = playlist[index] });
            }
            catch (Exception)
            {

                throw;
            }
        }

        /// <summary>
        /// 上一曲
        /// </summary>
        //private void SkipToPrevious()
        //{
        //    try
        //    {
        //        smtc.PlaybackStatus = MediaPlaybackStatus.Changing;
        //        currentIndex--;
        //        playbackList.MovePrevious();
        //    }
        //    catch
        //    {

        //    }
        //}
        private void SkipToPrevious()
        {
            try
            {
                var index = (currentIndex - 1 + playlist.Count) % playlist.Count;
                PlayIndex(index);
                CurrentPlayItemChanged?.Invoke(playlist, new CurrentMediaPlayItemChangedEventArgs() { NewItem = playlist[index] });
            }
            catch (Exception)
            {

                throw;
            }
        }

        /// <summary>
        /// 创建播放列表
        /// </summary>
        /// <param name="songs">更新播放列表的歌曲信息集合</param>
        private async void CreatePlaybackList(IEnumerable<SongModel> songs)
        {
            playbackList = new MediaPlaybackList();
            playbackList.AutoRepeatEnabled = true;

            //string json = JsonHelper.ToJson(songs);
            //StorageFile playListFile = await Package.Current.InstalledLocation.CreateFileAsync("playlist", CreationCollisionOption.ReplaceExisting);

            //using (var stream = await playListFile.OpenAsync(FileAccessMode.ReadWrite))
            //{
            //    StreamWriter writer = new StreamWriter(stream.AsStream());
            //    writer.Write(json);
            //    writer.Flush();
            //    writer.Dispose();
            //}

            foreach (var song in songs)
            {
                var file = await KnownFolders.MusicLibrary.GetFileAsync(song.FileName);

                var mediaSource = MediaSource.CreateFromStorageFile(file);

                mediaSource.CustomProperties["title"] = song.Title;
                mediaSource.CustomProperties["album"] = song.AlbumUri;
                mediaSource.CustomProperties["artist"] = song.Artist;
                mediaSource.CustomProperties["filename"] = song.FileName;

                playbackList.Items.Add(new MediaPlaybackItem(mediaSource));
            }

            BackgroundMediaPlayer.Current.AutoPlay = false;

            BackgroundMediaPlayer.Current.Source = playbackList;

            //playbackList.CurrentItemChanged += PlaybackList_CurrentItemChanged;

        }

        private void CreatePlaylist(IEnumerable<SongModel> songs)
        {
            if (playlist == null)
            {
                playlist = new List<SongModel>();
            }
            currentIndex = -1;
            if (playlist.Count > 0)
            {
                playlist.Clear();
            }

            foreach (var song in songs)
            {
                playlist.Add(song);
            }
            this.CurrentPlayItemChanged += BackgroundAudioTask_CurrentPlayItemChanged;
        }

        private void BackgroundAudioTask_CurrentPlayItemChanged(List<SongModel> playlist, CurrentMediaPlayItemChangedEventArgs args)
        {
            var item = args.NewItem;
            if (item != null)
            {
                UpdateUVCOnNewTrack((SongModel)item);
                MessageService.SendMessageToForeground(new TrackChangeMessage(currentIndex));
            }
        }

        private async void SetPlayItemSource(SongModel song)
        {
            try
            {
                var file = await KnownFolders.MusicLibrary.GetFileAsync(song.FileName);
                BackgroundMediaPlayer.Current.SetFileSource(file);
            }
            catch
            { }
        }

        private void PlayIndex(int index)
        {
            if (index >= 0 && index <= playlist.Count)
            {
                smtc.PlaybackStatus = MediaPlaybackStatus.Changing;
                SetPlayItemSource(playlist[index]);
                BackgroundMediaPlayer.Current.Play();
                smtc.PlaybackStatus = MediaPlaybackStatus.Playing;
                if (currentIndex != index)
                {
                    currentIndex = index;
                    CurrentPlayItemChanged?.Invoke(playlist, new CurrentMediaPlayItemChangedEventArgs() { NewItem = playlist[index] });
                }
            }
            else
            {
                Debug.WriteLine("The song index is out of range");
            }
        }

        private void PlayIndexWithPosition(int index, TimeSpan position)
        {
            if (index >= 0 && index <= playlist.Count)
            {
                smtc.PlaybackStatus = MediaPlaybackStatus.Changing;
                SetPlayItemSource(playlist[index]);
                BackgroundMediaPlayer.Current.Position = position;
                BackgroundMediaPlayer.Current.Play();
                smtc.PlaybackStatus = MediaPlaybackStatus.Playing;
                if (currentIndex != index)
                {
                    currentIndex = index;
                    CurrentPlayItemChanged?.Invoke(playlist, new CurrentMediaPlayItemChangedEventArgs() { NewItem = playlist[index] });
                }
            }
            else
            {
                Debug.WriteLine("The song index is out of range");
            }
        }

        /// <summary>
        /// 更新系统音乐控制器显示的media item
        /// </summary>
        /// <param name="item"></param>
        private void UpdateUVCOnNewTrack(SongModel item)
        {
            if (item == null)
            {
                smtc.PlaybackStatus = MediaPlaybackStatus.Stopped;
                smtc.DisplayUpdater.MusicProperties.Title = string.Empty;
                smtc.DisplayUpdater.Update();
                return;
            }

            smtc.PlaybackStatus = MediaPlaybackStatus.Playing;
            smtc.DisplayUpdater.Type = MediaPlaybackType.Music;
            smtc.DisplayUpdater.MusicProperties.Title = item.Title;

            var albumUri = item.AlbumUri;
            if (albumUri != null)
            {
                smtc.DisplayUpdater.Thumbnail = RandomAccessStreamReference.CreateFromUri(albumUri);
            }
            else
            {
                smtc.DisplayUpdater.Thumbnail = null;
            }
            smtc.DisplayUpdater.Update();
        }

        #endregion

    }
}
