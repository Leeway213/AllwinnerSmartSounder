using System.Runtime.Serialization;

namespace HeWeatherService.Models
{
    [DataContract]
    public class UpdateTime
    {
        [DataMember]
        public string loc { get; set; }

        [DataMember]
        public string utc { get; set; }
    }
}