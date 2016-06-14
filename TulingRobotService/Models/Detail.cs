using System.Runtime.Serialization;

namespace TulingRobotService.Models
{
    [DataContract]
    public class Detail
    {
        [DataMember]
        public string article { get; set; }

        [DataMember]
        public string source { get; set; }

        [DataMember]
        public string icon { get; set; }

        [DataMember]
        public string detailurl { get; set; }

        [DataMember]
        public string name { get; set; }

        [DataMember]
        public string info { get; set; }
    }
}