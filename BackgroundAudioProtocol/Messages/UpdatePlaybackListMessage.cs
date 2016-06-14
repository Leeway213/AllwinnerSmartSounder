using BackgroundAudioProtocol.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace BackgroundAudioProtocol.Messages
{
    /// <summary>
    /// 播放列表更新时，发送给后台任务的消息实体
    /// </summary>
    [DataContract]
    public class UpdatePlaybackListMessage
    {
        /// <summary>
        /// 新的播放列表的歌曲信息
        /// </summary>
        [DataMember]
        public List<SongModel> Songs { get; set; }

        /// <summary>
        /// 表示是否是第一次更新列表
        /// </summary>
        [DataMember]
        public bool IsResumed { get; set; }

        public UpdatePlaybackListMessage(List<SongModel> songs, bool isResumed)
        {
            this.Songs = songs;
            this.IsResumed = isResumed;
        }
    }
}
