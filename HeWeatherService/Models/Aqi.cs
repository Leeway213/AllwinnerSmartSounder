using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace HeWeatherService.Models
{
    [DataContract]
    public class Aqi
    {
        [DataMember]
        public AqiDetail city { get; set; }
    }
}
