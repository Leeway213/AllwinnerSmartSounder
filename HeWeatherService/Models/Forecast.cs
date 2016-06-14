using System.Runtime.Serialization;

namespace HeWeatherService.Models
{
    [DataContract]
    public class Forecast
    {
        [DataMember]
        public NowCondition cond { get; set; }

        [DataMember]
        public string date { get; set; }

        [DataMember]
        public double hum { get; set; }

        [DataMember]
        public double pcpn { get; set; }

        [DataMember]
        public double pop { get; set; }

        [DataMember]
        public double pres { get; set; }

        [DataMember]
        public double tmp { get; set; }

        [DataMember]
        public double vis { get; set; }

        [DataMember]
        public Wind wind { get; set; }

        [DataMember]
        public double fl { get; set; }
    }
}