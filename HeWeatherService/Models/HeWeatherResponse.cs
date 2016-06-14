using System.Collections.Generic;
using System.Runtime.Serialization;

namespace HeWeatherService.Models
{
    [DataContract]
    public class HeWeatherResponse
    {
        [DataMember]
        public Aqi aqi { get; set; }

        [DataMember]
        public BasicInfo basic { get; set; }

        [DataMember]
        public List<DailyForecast> daily_forecast { get; set; }

        [DataMember]
        public List<Forecast> hourly_forecast { get; set; }

        [DataMember]
        public Forecast now { get; set; }

        [DataMember]
        public string status { get; set; }

        [DataMember]
        public Suggestion suggestion { get; set; }
    }
}
