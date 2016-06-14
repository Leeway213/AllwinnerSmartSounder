using System;
using System.Runtime.Serialization;

namespace UDPService.Messages
{
    [DataContract]
    public class MusicInfo
    {
        [DataMember]
        public string Title { get; set; }

        [DataMember]
        public string Artist { get; set; }

        [DataMember]
        public TimeSpan Duration { get; set; }
    }
}