using System.Runtime.Serialization;

namespace IntelligentService.Models
{
    [DataContract]
    public class QueryResponse
    {

        [DataMember]
        public string query { get; set; }

        [DataMember]
        public Intent[] intents { get; set; }

        [DataMember]
        public Entity[] entities { get; set; }
    }
}
