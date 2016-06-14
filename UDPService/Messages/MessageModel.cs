using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UWPHelpers;
using Windows.Storage.Streams;

namespace UDPService.Messages
{
    /// <summary>
    /// 协议说明：
    ///     消息组成：[MessageType]      +       [RemotePort]      +   [TokenLength]     +   [Token]   +   [ContentLength] +   [Content]
    ///              长度: byte*1             长度:sizeof(uint)     长度: sizeof(uint) 
    ///     字段说明：
    ///     1. MessageType：标识消息类型。
    ///         1) 0x1a: DeviceDetection
    ///         2) 0x2b: 
    ///     2. TokenLength:标识Token长度
    ///     3. Token: 每个用户的Token(经过RC2_CBC加密)
    /// </summary>
    public class MessageModel
    {

        public static MessageModel ToMessageFromEncrypted(byte[] data)
        {
            MessageModel message = new MessageModel();

            try
            {
                var buffer = CryptographyHelper.Decrypt(data, CryptographyHelper.SECURITY_KEY);
                using (DataReader dataReader = DataReader.FromBuffer(buffer))
                {
                    if (dataReader.UnconsumedBufferLength == 0)
                    {
                        return null;
                    }
                    //Parse MessageType
                    byte typeByte = dataReader.ReadByte();
                    message.Type = ToMessageType(typeByte);

                    if (dataReader.UnconsumedBufferLength == 0)
                    {
                        return message;
                    }

                    uint remotePort = dataReader.ReadUInt32();
                    message.RemotePort = remotePort;

                    if (dataReader.UnconsumedBufferLength == 0)
                    {
                        return message;
                    }

                    uint tokenLength = dataReader.ReadUInt32();

                    if (dataReader.UnconsumedBufferLength == 0)
                    {
                        return message;
                    }

                    dataReader.UnicodeEncoding = Windows.Storage.Streams.UnicodeEncoding.Utf8;
                    message.Token = dataReader.ReadString(tokenLength);

                    if (dataReader.UnconsumedBufferLength == 0)
                    {
                        return message;
                    }

                    uint contentLength = dataReader.ReadUInt32();
                    message.ContentLength = contentLength;

                    if (dataReader.UnconsumedBufferLength == 0)
                    {
                        return message;
                    }

                    dataReader.UnicodeEncoding = Windows.Storage.Streams.UnicodeEncoding.Utf8;
                    message.Content = dataReader.ReadBuffer(contentLength);

                    return message;
                }
            }
            catch
            {
                return message;
            }
        }

        public static byte[] FromMessageWithEncrypted(MessageModel message)
        {
            if (message == null)
            {
                return null;
            }
            using (MemoryStream stream = new MemoryStream())
            {
                using (DataWriter writer = new DataWriter(stream.AsOutputStream()))
                {
                    writer.WriteByte(FromMessageType(message.Type));

                    writer.WriteUInt32(message.RemotePort);

                    if (!string.IsNullOrEmpty(message.Token))
                    {
                        writer.WriteUInt32(writer.MeasureString(message.Token));
                        writer.UnicodeEncoding = Windows.Storage.Streams.UnicodeEncoding.Utf8;
                        writer.WriteString(message.Token);
                    }
                    writer.WriteUInt32(message.ContentLength);
                    if (message.Content != null)
                    {
                        writer.WriteBuffer(message.Content);
                    }

                    var ignore = writer.StoreAsync();

                    return CryptographyHelper.Encrypt(stream.ToArray(), CryptographyHelper.SECURITY_KEY);
                }
            }
                
        }

        public static MessageModel ToMessage(DataReader dataReader)
        {
            MessageModel message = new MessageModel();
            try
            {
                if (dataReader.UnconsumedBufferLength == 0)
                {
                    return null;
                }
                //Parse MessageType
                byte typeByte = dataReader.ReadByte();
                message.Type = ToMessageType(typeByte);

                if (dataReader.UnconsumedBufferLength == 0)
                {
                    return message;
                }

                uint remotePort = dataReader.ReadUInt32();
                message.RemotePort = remotePort;

                if (dataReader.UnconsumedBufferLength == 0)
                {
                    return message;
                }

                uint tokenLength = dataReader.ReadUInt32();

                if (dataReader.UnconsumedBufferLength == 0)
                {
                    return message;
                }

                dataReader.UnicodeEncoding = Windows.Storage.Streams.UnicodeEncoding.Utf8;
                message.Token = dataReader.ReadString(tokenLength);

                if (dataReader.UnconsumedBufferLength == 0)
                {
                    return message;
                }

                uint contentLength = dataReader.ReadUInt32();
                message.ContentLength = contentLength;

                if (dataReader.UnconsumedBufferLength == 0)
                {
                    return message;
                }

                dataReader.UnicodeEncoding = Windows.Storage.Streams.UnicodeEncoding.Utf8;
                message.Content = dataReader.ReadBuffer(contentLength);

                return message;
            }
            catch (Exception)
            {
                return message;
            }
        }

        public static byte[] FromMessage(MessageModel message)
        {
            if (message == null)
            {
                return null;
            }
            using (MemoryStream stream = new MemoryStream())
            {
                using (DataWriter writer = new DataWriter(stream.AsOutputStream()))
                {
                    writer.WriteByte(FromMessageType(message.Type));
                    writer.WriteUInt32(message.RemotePort);

                    if (!string.IsNullOrEmpty(message.Token))
                    {
                        writer.WriteUInt32(writer.MeasureString(message.Token));
                        writer.UnicodeEncoding = Windows.Storage.Streams.UnicodeEncoding.Utf8;
                        writer.WriteString(message.Token);
                    }
                    writer.WriteUInt32(message.ContentLength);
                    if (message.Content != null)
                    {
                        writer.WriteBuffer(message.Content);
                    }

                    var ignore = writer.StoreAsync();

                    return stream.ToArray();
                }
            }
        }

        public MessageType Type { get; set; }

        public uint RemotePort { get; set; }

        public string Token { get; set; }

        public uint ContentLength { get; set; }

        public IBuffer Content { get; set; }

        private static MessageType ToMessageType(byte type)
        {
            switch (type)
            {
                case 0x1a:
                    return MessageType.DeviceDetection;
                case 0x1b:
                    return MessageType.WiFiConnect;
                case 0x1c:
                    return MessageType.UpdateAll;
                case 0x1d:
                    return MessageType.PlayerStateChanged;
                case 0x1e:
                    return MessageType.UpdatePlaylist;
                case 0x1f:
                    return MessageType.UpdateTrack;
                case 0x2a:
                    return MessageType.UpdatePosition;
                case 0x2b:
                    return MessageType.ChangeVolume;
                default:
                    return MessageType.Unknown;
            }
        }

        private static byte FromMessageType(MessageType type)
        {
            switch (type)
            {
                case MessageType.DeviceDetection:
                    return 0x1a;
                case MessageType.WiFiConnect:
                    return 0x1b;
                case MessageType.UpdateAll:
                    return 0x1c;
                case MessageType.PlayerStateChanged:
                    return 0x1d;
                case MessageType.UpdatePlaylist:
                    return 0x1e;
                case MessageType.UpdateTrack:
                    return 0x1f;
                case MessageType.UpdatePosition:
                    return 0x2a;
                case MessageType.ChangeVolume:
                    return 0x2b;
                default:
                    return 0x00;
            }
        }
    }


    public enum MessageType
    {
        DeviceDetection, WiFiConnect, Unknown, UpdateAll, PlayerStateChanged, UpdatePlaylist, UpdateTrack, UpdatePosition, ChangeVolume
    }
}
