
using System.Runtime.Serialization;

namespace BackgroundAudioProtocol.Messages
{
    /// <summary>
    /// 后台音频track信息改变时，发送给前台的消息实体
    /// </summary>
    [DataContract]
    public class TrackChangeMessage
    {
        /// <summary>
        /// 表示新的当前播放的歌曲在播放列表中的索引
        /// </summary>
        [DataMember]
        public int TrackIndex { get; set; }

        [DataMember]
        public string TrackFile { get; set; }


        public TrackChangeMessage(int index)
        {
            this.TrackIndex = index;
        }

        public TrackChangeMessage(string file)
        {
            this.TrackFile = file;
        }
    }
}
