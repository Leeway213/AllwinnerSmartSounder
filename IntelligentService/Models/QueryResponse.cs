using System.Runtime.Serialization;

namespace IntelligentService.Models
{
    [DataContract]
    public class QueryResponse
    {

        [DataMember]
        public string query { get; set; }

        [DataMember]
        public Intent topScoringIntent { get; set; }

        [DataMember]
        public Entity[] entities { get; set; }
    }
}
