using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace HeWeatherService.Models
{
    [DataContract]
    public class AqiDetail
    {
        [DataMember]
        public int aqi { get; set; }

        [DataMember]
        public int co { get; set; }

        [DataMember]
        public int no2 { get; set; }

        [DataMember]
        public int o3 { get; set; }

        [DataMember]
        public int pm10 { get; set; }

        [DataMember]
        public int pm25 { get; set; }

        [DataMember]
        public string qlty { get; set; }

        [DataMember]
        public int so2 { get; set; }
    }
}
