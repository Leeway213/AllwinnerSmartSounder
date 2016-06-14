using System;
using System.Runtime.Serialization;
using Windows.Storage.Streams;

namespace BackgroundAudioProtocol.Models
{
    /// <summary>
    /// 包含歌曲的名称、艺术家、流派、文件路径等信息
    /// </summary>
    [DataContract]
    public class SongModel
    {
        [DataMember]
        public string Title { get; set; }

        [DataMember]
        public Uri MediaUri { get; set; }

        [DataMember]
        public Uri AlbumUri { get; set; }

        [DataMember]
        public string FileName { get; set; }

        [DataMember]
        public string Artist { get; set; }

        [DataMember]
        public string[] Genre { get; set; }

        [DataMember]
        public TimeSpan Duration { get; set; }

    }
}
