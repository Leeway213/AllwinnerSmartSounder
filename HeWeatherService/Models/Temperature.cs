using System.Runtime.Serialization;

namespace HeWeatherService.Models
{
    [DataContract]
    public class Temperature
    {
        [DataMember]
        public double max { get; set; }

        [DataMember]
        public double min { get; set; }
    }
}