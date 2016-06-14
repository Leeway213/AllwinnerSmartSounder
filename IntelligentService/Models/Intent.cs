using System.Runtime.Serialization;

namespace IntelligentService.Models
{
    [DataContract]
    public class Intent
    {
        [DataMember]
        public string intent { get; set; }

        [DataMember]
        public double score { get; set; }

    }
}
