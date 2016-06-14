using System.Runtime.Serialization;

namespace HeWeatherService.Models
{
    [DataContract]
    public class SunInfo
    {
        [DataMember]
        public string sr { get; set; }

        [DataMember]
        public string ss { get; set; }
    }
}