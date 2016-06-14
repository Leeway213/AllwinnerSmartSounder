using System.Runtime.Serialization;

namespace HeWeatherService.Models
{
    [DataContract]
    public class BasicInfo
    {
        [DataMember]
        public string city { get; set; }

        [DataMember]
        public string cnty { get; set; }

        [DataMember]
        public string id { get; set; }

        [DataMember]
        public double lat { get; set; }

        [DataMember]
        public double lon { get; set; }

        [DataMember]
        public UpdateTime update { get; set; }
    }
}