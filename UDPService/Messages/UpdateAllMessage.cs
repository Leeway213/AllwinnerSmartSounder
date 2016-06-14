using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace UDPService.Messages
{
    [DataContract]
    public class UpdateAllMessage
    {
        [DataMember]
        public string PlayerState { get; set; }

        [DataMember]
        public int TrackIndex { get; set; }

        [DataMember]
        public List<MusicInfo> Playlist { get; set; }

        [DataMember]
        public TimeSpan SeekPosition { get; set; }

        [DataMember]
        public double Volume { get; set; }
    }
}
