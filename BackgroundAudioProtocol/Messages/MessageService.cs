using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UWPHelpers;
using Windows.Foundation.Collections;
using Windows.Media.Playback;

namespace BackgroundAudioProtocol.Messages
{
    /// <summary>
    /// 前台APP与后台音频任务相互发送消息的帮助类
    /// </summary>
    public static class MessageService
    {
        const string MessageType = "MessageType";
        const string MessageBody = "MessageBody";

        /// <summary>
        /// 发送消息到前台
        /// </summary>
        /// <typeparam name="T">消息类型</typeparam>
        /// <param name="message">消息实体</param>
        public static void SendMessageToForeground<T>(T message)
        {
            var playload = GetMessageValueSet(message);
            BackgroundMediaPlayer.SendMessageToForeground(playload);
        }

        /// <summary>
        /// 发送消息到后台任务
        /// </summary>
        /// <typeparam name="T">消息类型</typeparam>
        /// <param name="message">消息实体</param>
        public static void SendMessageToBackground<T>(T message)
        {
            var playload = GetMessageValueSet(message);
            BackgroundMediaPlayer.SendMessageToBackground(playload);
        }

        /// <summary>
        /// 解析消息内容
        /// </summary>
        /// <typeparam name="T">供输出消息的类型</typeparam>
        /// <param name="valueSet">消息序列</param>
        /// <param name="message">接收消息的实体</param>
        /// <returns>返回true，则解析消息成功，message为输出的消息实体</returns>
        public static bool TryParseMessage<T>(ValueSet valueSet, out T message)
        {
            object messageTypeValue;
            object messageBodyValue;
            message = default(T);

            if (valueSet.TryGetValue(MessageService.MessageType, out messageTypeValue)
                && valueSet.TryGetValue(MessageService.MessageBody, out messageBodyValue))
            {
                if ((string)messageTypeValue != typeof(T).FullName)
                {
                    return false;
                }

                message = JsonHelper.FromJson<T>(messageBodyValue.ToString());
                return true;
            }
            return false;
        }

        private static ValueSet GetMessageValueSet<T>(T message)
        {
            ValueSet vs = new ValueSet();
            vs.Add(MessageService.MessageType, typeof(T).FullName);
            vs.Add(MessageService.MessageBody, JsonHelper.ToJson(message));
            return vs;
        }

    }
}
