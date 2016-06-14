using System.Runtime.Serialization;

namespace HeWeatherService.Models
{
    [DataContract]
    public class NowCondition
    {
        [DataMember]
        public int code { get; set; }
        
        [DataMember]
        public string txt { get; set; }
    }
}