using System.Runtime.Serialization;

namespace IntelligentService.Models
{
    [DataContract]
    public class Resolution
    {
        [DataMember]
        public string resolution_type { get; set; }

        [DataMember]
        public string date { get; set; }

        [DataMember]
        public string time { get; set; }

        [DataMember]
        public string duration { get; set; }

        [DataMember]
        public string metadataType { get; set; }

        [DataMember]
        public string value { get; set; }

    }
}
