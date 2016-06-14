using BackgroundAudioProtocol.BackgroundAudioSettings;
using BackgroundAudioProtocol.Messages;
using BackgroundAudioProtocol.Models;
using GalaSoft.MvvmLight.Threading;
using IntelligentService.Models;
using SmartSounder.ViewModel;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UDPService;
using UWPHelpers;
using Windows.Foundation;
using Windows.Media.Playback;
using Windows.Storage;
using Windows.UI.Xaml.Controls;

namespace SmartSounder.Tools
{
    /// <summary>
    /// 后台音频的媒体控制器
    /// </summary>
    public class MediaController
    {
        #region Private Fields

        /// <summary>
        /// 获取后台音频失败的HResult code
        /// </summary>
        private const int RPC_S_SERVER_UNAVAILABLE = -2147023174;   //-2147023174

        private object o = new object();


        #endregion

        #region Public Fields

        public AutoResetEvent BackgroundAudioTaskStarted;

        #endregion

        #region Properties

        private int _currentPlayingIndex;
        /// <summary>
        /// 当前播放音乐在播放列表中的索引
        /// </summary>
        public int CurrentPlayingIndex
        {
            get { return _currentPlayingIndex; }
            set
            {
                if (_currentPlayingIndex != value && value >= 0)
                {
                    _currentPlayingIndex = value;

                    ChangeTrack(value);
                }
            }
        }

        /// <summary>
        /// 是否正在播放
        /// </summary>
        public bool IsPlaying
        {
            get
            {
                if (CurrentPlayer.CurrentState != MediaPlayerState.Closed &&
                    CurrentPlayer.CurrentState != MediaPlayerState.Stopped &&
                    CurrentPlayer.CurrentState != MediaPlayerState.Paused)
                {
                    return true;
                }
                return false;
            }
        }

        private List<SongModel> _songList;
        /// <summary>
        /// 当前播放列表
        /// </summary>
        public List<SongModel> SongList
        {
            get
            {
                if (_songList == null)
                {
                    _songList = new List<SongModel>();
                }
                return _songList;
            }
            set
            {
                _songList = value;
            }
        }

        private List<SongModel> _playlist;

        public List<SongModel> Playlist
        {
            get
            {
                if (_playlist == null)
                {
                    _playlist = new List<SongModel>();
                }
                return _playlist;
            }
            set
            {
                _playlist = value;
            }
        }


        private static MediaController _current;
        /// <summary>
        /// MediaController实例(单例模式)
        /// </summary>
        public static MediaController Current
        {
            get
            {
                if (_current == null)
                {
                    _current = new MediaController();
                }
                return _current;
            }
        }


        private bool _isBackgroundRunning = false;
        /// <summary>
        /// 后台音频任务是否处于运行状态
        /// </summary>
        public bool IsBackgroundRunning
        {
            get
            {
                if (_isBackgroundRunning)
                {
                    return true;
                }

                string backgroungState = BackgroundAudioSettingsHelper.GetValue(BackgroundAudioSettingsConstants.BACKGROUND_TASK_STATE) as string;
                if (backgroungState != null)
                {
                    try
                    {
                        var state = EnumHelper.Parse<BackgroundTaskState>(backgroungState);
                        _isBackgroundRunning = state == BackgroundTaskState.Running;
                    }
                    catch (Exception ex)
                    {
                        _isBackgroundRunning = false;
                    }
                    return _isBackgroundRunning;
                }
                else
                    return false;
            }
        }

        /// <summary>
        /// 后台音频播放器实例
        /// </summary>
        public MediaPlayer CurrentPlayer
        {
            get
            {
                MediaPlayer mp = null;
                int retryCount = 2;

                while (mp == null && retryCount-- > 0)
                {
                    try
                    {
                        mp = BackgroundMediaPlayer.Current;
                    }
                    catch (Exception ex)
                    {
                        if (ex.HResult == RPC_S_SERVER_UNAVAILABLE)
                        {
                            ResetAfterLostBackground();
                            StartBackgroundAudioTask();
                        }
                        else
                            throw;
                    }
                }

                if (mp == null)
                    throw new Exception("Fail to get a meidia player instance");
                return mp;
            }
        }

        #endregion

        #region Event Handlers

        /// <summary>
        /// 后台音频播放器状态改变的event handler(外部调用)
        /// </summary>
        public event TypedEventHandler<MediaPlayer, object> CurrentPlayerStateChanged
        {
            add { CurrentPlayer.CurrentStateChanged += value; }
            remove { CurrentPlayer.CurrentStateChanged -= value; }
        }

        /// <summary>
        /// 接收到后台消息的event handler(外部调用)
        /// </summary>
        public event EventHandler<MediaPlayerDataReceivedEventArgs> MessageReceivedFromBackground
        {
            add { BackgroundMediaPlayer.MessageReceivedFromBackground += value; }
            remove { BackgroundMediaPlayer.MessageReceivedFromBackground -= value; }
        }

        public delegate void PlaylistChangedEventHandler(MediaController sender, PlaylistChangedEventArgs args);
        public event PlaylistChangedEventHandler PlaylistChanged;

        public delegate void TrackChangedEventHandler(MediaController sender, TrackChangedEventArgs args);
        public event TrackChangedEventHandler TrackChanged;

        /// <summary>
        /// 接收到后台消息时的event handler
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BackgroundMediaPlayer_MessageReceivedFromBackground(object sender, MediaPlayerDataReceivedEventArgs e)
        {

            lock (o)
            {
                TrackChangeMessage trackChangeMessage;
                if (MessageService.TryParseMessage(e.Data, out trackChangeMessage))
                {
                    TrackChangedEventArgs args = new TrackChangedEventArgs();
                    args.OldTrack = CurrentPlayingIndex;
                    args.NewTrack = trackChangeMessage.TrackIndex;
                    TrackChanged?.Invoke(this, args);
                    DispatcherHelper.CheckBeginInvokeOnUI(() =>
                                        {
                                            new ViewModelLocator().Main.CurrentIndex = trackChangeMessage.TrackIndex;
                                        });
                    CurrentPlayingIndex = trackChangeMessage.TrackIndex;
                    //int index = Playlist.FindIndex(p => p.FileName == trackChangeMessage.TrackFile);
                    //TrackChangedEventArgs args = new TrackChangedEventArgs();
                    //args.OldTrack = CurrentPlayingIndex;
                    //args.NewTrack = index;
                    //TrackChanged?.Invoke(this, args);

                    //CurrentPlayingIndex = index;
                    return;
                }

                BackgroundAudioTaskStopedMessage backgroundAudioTaskStopedMessage;
                if (MessageService.TryParseMessage(e.Data, out backgroundAudioTaskStopedMessage))
                {
                    return;
                }
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// 执行播放命令
        /// </summary>
        /// <returns>是否成功播放</returns>
        public bool Play()
        {
            if (SongList == null || SongList.Count == 0)
            {
                return false;
            }
            MessageService.SendMessageToBackground(new StartPlaybackMessage());

            return true;
        }

        /// <summary>
        /// 根据提供的意图实体，播放特定的音乐，如：歌手、歌曲名、流派
        /// </summary>
        /// <param name="entities">包含歌手、歌曲名、流派的意图实体</param>
        /// <returns>如果找到了相关的音乐并成功播放，返回true；否则返回false</returns>
        public bool Play(Entity[] entities)
        {
            //删除保存的歌曲进度
            BackgroundAudioSettingsHelper.DeleteValue(BackgroundAudioSettingsConstants.TRACK_INDEX);
            BackgroundAudioSettingsHelper.DeleteValue(BackgroundAudioSettingsConstants.POSITION);

            //循环意图实体数组，根据意图实体筛选新的播放列表
            IEnumerable<SongModel> newList = SongList;
            foreach (var entity in entities)
            {
                Debug.WriteLine("---------Play:" + entity.entity);

                var type = entity.type;

                //根据艺术家名称进行筛选
                if (type.Equals("sounder.media.music_artist_name") ||
                    type.Equals("builtin.ondevice.music_artist_name"))
                {
                    newList = newList.Where(p =>
                            p.Artist.ToLower().
                            Contains(entity.entity.EndsWith("\'") ?
                            entity.entity.Substring(0, entity.entity.Length - 1).ToLower() :
                            entity.entity.ToLower()));
                }

                //根据歌曲名进行筛选
                else if (type.Equals("builtin.ondevice.music_song_name") ||
                    type.Equals("sounder.media.music_song_name"))
                {
                    newList = newList.Where(p => string.Equals(p.Title, entity.entity, StringComparison.CurrentCultureIgnoreCase));
                }

                //根据流派进行筛选
                else if (type.Equals("sounder.media.music_genre") || type.Equals("builtin.ondevice.music_genre"))
                {
                    newList = newList.Where(p => p.Genre.Contains(entity.entity));
                }
                //switch (entity.type)
                //{
                //    case "sounder.media.music_artist_name":
                //    case "builtin.ondevice.music_artist_name":
                //        newList = newList.Where(p =>
                //            p.Artist.ToLower().
                //            Contains(entity.entity.EndsWith("\'") ?
                //            entity.entity.Substring(0, entity.entity.Length - 1) :
                //            entity.entity));

                //        break;

                //    case "sounder.media.music_genre":
                //    case "builtin.ondevice.music_genre":
                //        newList = newList.Where(p => p.Genre.Contains(entity.entity));
                //        break;
                //    case "builtin.ondevice.music_song_name":
                //        newList = newList.Where(p => string.Equals(p.Title, entity.entity, StringComparison.CurrentCultureIgnoreCase));
                //        break;
                //    default:
                //        break;
                //}

            }

            //如果筛选后的list为空，说明未找到指定的音乐，返回false
            if (newList == null || newList.Count() == 0)
            {
                return false;
            }

            //否则更新后台音频的播放列表，并开始播放
            UpdatePlaybackList(newList.ToList(), false);
            MessageService.SendMessageToBackground(new StartPlaybackMessage());
            return true;


        }

        /// <summary>
        /// 执行暂停命令
        /// </summary>
        public void Pause()
        {
            if (IsBackgroundRunning)
            {
                BackgroundAudioSettingsHelper.SetValue(BackgroundAudioSettingsConstants.TRACK_INDEX, CurrentPlayingIndex);
                BackgroundAudioSettingsHelper.SetValue(BackgroundAudioSettingsConstants.POSITION, CurrentPlayer.Position.ToString());
                CurrentPlayer.Pause();
            }
        }

        /// <summary>
        /// 执行下一曲命令
        /// </summary>
        public void Next()
        {
            MessageService.SendMessageToBackground(new SkipNextMessage());
        }

        /// <summary>
        /// 执行上一曲命令
        /// </summary>
        public void Previous()
        {
            MessageService.SendMessageToBackground(new SkipPreviousMessage());
        }

        /// <summary>
        /// 更新后台音频播放列表
        /// </summary>
        /// <param name="songs">更新的歌曲信息</param>
        /// <param name="isResumed">是否是第一次更新列表</param>
        public void UpdatePlaybackList(List<SongModel> songs, bool isResumed)
        {
            MessageService.SendMessageToBackground(new UpdatePlaybackListMessage(songs, isResumed));
            Playlist.Clear();
            foreach (var song in songs)
            {
                Playlist.Add(song);
            }
            PlaylistChanged?.Invoke(this, new PlaylistChangedEventArgs()
            {
                NewList = songs
            });
        }

        /// <summary>
        /// 改变当前播放歌曲
        /// </summary>
        /// <param name="index">歌曲索引</param>
        public void ChangeTrack(int index)
        {
            lock (o)
            {
                Debug.WriteLine("------------改变" + index);
                //string message = Playlist[index].FileName;
                MessageService.SendMessageToBackground(new TrackChangeMessage(index));
            }
        }

        /// <summary>
        /// 调整歌曲播放进度
        /// </summary>
        /// <param name="position">调整后的事件点</param>
        public void Seek(TimeSpan position)
        {
            CurrentPlayer.Position = position;
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// 后台任务异常关闭时，重新启动关闭的后台任务
        /// </summary>
        private void ResetAfterLostBackground()
        {
            BackgroundMediaPlayer.Shutdown();
            _isBackgroundRunning = false;
            BackgroundAudioTaskStarted.Reset();

            BackgroundAudioSettingsHelper.SetValue(BackgroundAudioSettingsConstants.BACKGROUND_TASK_STATE, BackgroundTaskState.Unknown.ToString());

            try
            {
                BackgroundMediaPlayer.MessageReceivedFromBackground += BackgroundMediaPlayer_MessageReceivedFromBackground;
            }
            catch (Exception ex)
            {
                if (ex.HResult == RPC_S_SERVER_UNAVAILABLE)
                {
                    throw new Exception("Fail to get a media instance", ex);
                }
                throw ex;
            }
        }

        /// <summary>
        /// 为后台播放器添加必要的事件处理器
        /// </summary>
        private void AddMediaPlayEventHandlers()
        {
            //CurrentPlayer.CurrentStateChanged += CurrentPlayer_CurrentStateChanged;

            try
            {
                BackgroundMediaPlayer.MessageReceivedFromBackground += BackgroundMediaPlayer_MessageReceivedFromBackground;
            }
            catch (Exception ex)
            {
                if (ex.HResult == RPC_S_SERVER_UNAVAILABLE)
                {
                    ResetAfterLostBackground();
                }
                else
                    throw;
            }
        }

        /// <summary>
        /// 启动后台音频
        /// </summary>
        private void StartBackgroundAudioTask()
        {
            AddMediaPlayEventHandlers();

            //var startResult = App.MainFrame.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            //{
            //    bool result = backgroundAudioTaskStarted.WaitOne(10000);
            //    if (result)
            //    {
            //        MessageService.SendMessageToBackground(new UpdatePlaybackListMessage(new List<BackgroundAudioProtocol.Models.SongModel>()));
            //        MessageService.SendMessageToBackground(new StartPlaybackMessage());
            //    }
            //    else
            //    {
            //        throw new Exception("Background task didn't start in expect time");
            //    }
            //});

            //startResult.Completed = BackgroundTaskStartCompleted;
        }

        //public event AsyncActionCompletedHandler BackgroundTaskStartCompleted;

        #endregion

        #region Constructs

        private MediaController()
        {
            BackgroundAudioTaskStarted = new AutoResetEvent(false);
            StartBackgroundAudioTask();
        }

        #endregion
    }
}
