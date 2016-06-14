using System.Runtime.Serialization;

namespace HeWeatherService.Models
{
    [DataContract]
    public class Condition
    {
        [DataMember]
        public int code_d { get; set; }

        [DataMember]
        public int code_n { get; set; }

        [DataMember]
        public string txt_d { get; set; }

        [DataMember]
        public string txt_n { get; set; }
    }
}