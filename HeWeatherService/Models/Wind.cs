using System.Runtime.Serialization;

namespace HeWeatherService.Models
{
    [DataContract]
    public class Wind
    {
        [DataMember]
        public double deg { get; set; }

        [DataMember]
        public string dir { get; set; }

        [DataMember]
        public string sc { get; set; }

        [DataMember]
        public double spd { get; set; }
    }
}