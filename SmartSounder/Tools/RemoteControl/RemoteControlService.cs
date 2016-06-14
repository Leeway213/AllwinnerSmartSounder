using BackgroundAudioProtocol.Messages;
using BackgroundAudioProtocol.Models;
using GalaSoft.MvvmLight.Threading;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UDPService;
using UDPService.Messages;
using UWPHelpers;
using Windows.Media.Playback;
using Windows.Security.Cryptography;
using Windows.Storage.Streams;
using Windows.UI.Xaml;

namespace SmartSounder.Tools.RemoteControl
{
    public class RemoteControlService
    {
        private static object o = new object();
        private static DispatcherTimer _positionUpdateTimer;
        public static void StartService()
        {
            try
            {
                _positionUpdateTimer = new DispatcherTimer();
                _positionUpdateTimer.Interval = TimeSpan.FromSeconds(1);
                _positionUpdateTimer.Tick += _positionUpdateTimer_Tick;
                UDPServer.Current.MessageReceived += Current_MessageReceived;
                MediaController.Current.PlaylistChanged += Current_PlaylistChanged;
                MediaController.Current.TrackChanged += Current_TrackChanged;
                MediaController.Current.CurrentPlayerStateChanged += Current_CurrentPlayerStateChanged;
                MediaController.Current.CurrentPlayer.VolumeChanged += CurrentPlayer_VolumeChanged;

            }
            catch { }
        }


        private static void _positionUpdateTimer_Tick(object sender, object e)
        {
            if (UDPServer.Current.ClientList?.Count > 0 && _positionUpdateTimer.IsEnabled)
            {
                SendUpdatePosition(MediaController.Current.CurrentPlayer.Position);
            }
        }

        private static void Current_TrackChanged(MediaController sender, TrackChangedEventArgs args)
        {
            if (UDPServer.Current.ClientList?.Count > 0)
            {
                SendTrackChanged(args.NewTrack);
            }
        }

        private static void Current_PlaylistChanged(MediaController sender, PlaylistChangedEventArgs args)
        {
            if (UDPServer.Current.ClientList?.Count > 0)
            {
                SendUpdateList(args.NewList);
            }
        }

        private static void CurrentPlayer_VolumeChanged(Windows.Media.Playback.MediaPlayer sender, object args)
        {
            SendVolumeChanged(sender.Volume);
        }

        private static void SendVolumeChanged(double volume)
        {
            MessageModel message = new MessageModel();
            message.Type = MessageType.ChangeVolume;
            using (DataWriter writer = new DataWriter())
            {
                writer.WriteDouble(volume);
                message.Content = writer.DetachBuffer();
                message.ContentLength = sizeof(double);
                SendData(message);
            }
        }

        private static void Current_CurrentPlayerStateChanged(Windows.Media.Playback.MediaPlayer sender, object args)
        {
            if (UDPServer.Current.ClientList?.Count > 0)
            {
                SendPlayerStateChanged(sender.CurrentState);
            }
            DispatcherHelper.CheckBeginInvokeOnUI(() =>
            {
                if (sender.CurrentState == MediaPlayerState.Playing)
                {
                    SendTrackChanged(MediaController.Current.CurrentPlayingIndex);
                    _positionUpdateTimer?.Start();
                }
                else
                {
                    _positionUpdateTimer?.Stop();
                }
            });
        }

        private static void Current_MessageReceived(Windows.Networking.Sockets.DatagramSocket sender, MessageModel message)
        {
            lock (o)
            {
                var client = UDPServer.Current.ClientList.FirstOrDefault(p => p.Token.Equals(message.Token));
                if (client != null)
                {
                    //if (!client.HostName.Equals(args.RemoteAddress.RawName))
                    //{
                    //    client.HostName = args.RemoteAddress.RawName;
                    //}
                    switch (message.Type)
                    {
                        case MessageType.UpdateAll:
                            SendAll(client);
                            break;
                        case MessageType.PlayerStateChanged:
                            ChangePlayerState(message.Content);
                            break;
                        case MessageType.UpdateTrack:
                            DataReader reader = DataReader.FromBuffer(message.Content);
                            int index = reader.ReadInt32();
                            Debug.WriteLine("接收到的track为：  " + index);
                            if (index >= 0 && index <= MediaController.Current.Playlist.Count)
                            {
                                MediaController.Current.ChangeTrack(index);
                            }
                            break;
                        case MessageType.UpdatePosition:
                            break;
                        case MessageType.ChangeVolume:
                            break;
                        default:
                            break;
                    }
                }
            }
        }

        private static void SendUpdatePosition(TimeSpan position)
        {
            MessageModel _sendData = new MessageModel();
            _sendData.Type = MessageType.UpdatePosition;
            _sendData.Content = CryptographicBuffer.ConvertStringToBinary(position.ToString(), BinaryStringEncoding.Utf8);
            _sendData.ContentLength = _sendData.Content.Capacity;
            SendData(_sendData);
        }

        private static void SendTrackChanged(int newTrack)
        {
            MessageModel _sendData = new MessageModel();
            _sendData.Type = MessageType.UpdateTrack;
            DataWriter writer = new DataWriter();
            writer.WriteInt32(newTrack);
            _sendData.Content = writer.DetachBuffer();
            _sendData.ContentLength = sizeof(Int32);
            SendData(_sendData);
        }

        private static void SendPlayerStateChanged(MediaPlayerState state)
        {
            MessageModel _sendData = new MessageModel();
            _sendData.Type = MessageType.PlayerStateChanged;
            _sendData.Content = CryptographicBuffer.ConvertStringToBinary(state.ToString(), BinaryStringEncoding.Utf8);
            _sendData.ContentLength = _sendData.Content.Capacity;
            SendData(_sendData);
        }

        private static void SendUpdateList(List<SongModel> list)
        {
            MessageModel _sendData = new MessageModel();
            _sendData.Type = MessageType.UpdatePlaylist;
            _sendData.Content = CryptographicBuffer.ConvertStringToBinary(JsonHelper.ToJson<List<SongModel>>(list), BinaryStringEncoding.Utf8);
            _sendData.ContentLength = _sendData.Content.Capacity;
            SendData(_sendData);
        }

        private static void SendData(MessageModel _sendData)
        {
            foreach (var client in UDPServer.Current.ClientList)
            {
                _sendData.Token = client.Token;
                _sendData.RemotePort = client.RemotePort;
                UDPClient.SendData(client.HostName, client.RemotePort.ToString(), MessageModel.FromMessage(_sendData));
            }
        }

        private static void SendAll(ClientInfo client)
        {
            MessageModel _sendData = new MessageModel();
            _sendData.Type = MessageType.UpdateAll;
            _sendData.Token = client.Token;
            _sendData.RemotePort = client.RemotePort;

            UpdateAllMessage message = new UpdateAllMessage()
            {
                PlayerState = MediaController.Current.CurrentPlayer.CurrentState.ToString(),
                TrackIndex = MediaController.Current.CurrentPlayingIndex,
                SeekPosition = MediaController.Current.CurrentPlayer.Position,
                Volume = MediaController.Current.CurrentPlayer.Volume
            };

            List<MusicInfo> playlist = new List<MusicInfo>();
            foreach (var item in MediaController.Current.SongList)
            {
                playlist.Add(new MusicInfo()
                {
                    Artist = item.Artist,
                    Title = item.Title,
                    Duration = item.Duration
                });
            }
            message.Playlist = playlist;

            string json = JsonHelper.ToJson(message);
            _sendData.Content = CryptographicBuffer.ConvertStringToBinary(json, BinaryStringEncoding.Utf8);
            _sendData.ContentLength = _sendData.Content.Capacity;
            UDPClient.SendData(client.HostName, client.RemotePort.ToString(), MessageModel.FromMessage(_sendData));
        }

        private static void ChangePlayerState(IBuffer content)
        {
            using (DataReader reader = DataReader.FromBuffer(content))
            {
                reader.UnicodeEncoding = Windows.Storage.Streams.UnicodeEncoding.Utf8;
                string str = reader.ReadString(content.Length);
            }
        }
    }
}
