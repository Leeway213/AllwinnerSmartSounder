using System.Runtime.Serialization;

namespace TulingRobotService.Models
{
    [KnownType(typeof(TulingRobotService.Models.RequestData))]
    [DataContract]
    public class RequestData
    {
        [DataMember]
        public string key { get; set; }

        [DataMember]
        public string info { get; set; }

        [DataMember]
        public string userid { get; set; }

    }
}
