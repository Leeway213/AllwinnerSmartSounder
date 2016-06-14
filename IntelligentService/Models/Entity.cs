using System.Runtime.Serialization;

namespace IntelligentService.Models
{
    [DataContract]
    public class Entity
    {
        [DataMember]
        public string entity { get; set; }

        [DataMember]
        public string type { get; set; }

        [DataMember]
        public Resolution resolution { get; set; }
    }
}
