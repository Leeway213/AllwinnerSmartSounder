using System.Runtime.Serialization;
using System.Text;
using Windows.Globalization;

namespace HeWeatherService.Models
{
    [DataContract]
    public class DailyForecast
    {
        [DataMember]
        public SunInfo astro { get; set; }

        [DataMember]
        public Condition cond { get; set; }

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
        public Temperature tmp { get; set; }

        [DataMember]
        public double vis { get; set; }

        [DataMember]
        public Wind wind { get; set; }

        [DataMember]
        public double fl { get; set; }

        public string WeatherText
        {
            get
            {
                var topUserLanguage = Windows.System.UserProfile.GlobalizationPreferences.Languages[0];
                var languange = new Language(topUserLanguage);
                if (languange.LanguageTag.Contains("zh"))
                {
                    return string.Format("{0}，气温{1}到{2}℃，{3}，风力{4}。",
                                    cond.code_d == cond.code_n ? cond.txt_d : new StringBuilder().Append(cond.txt_d).Append("转").Append(cond.txt_n).ToString(),
                                    tmp.min,
                                    tmp.max,
                                    wind.dir.Equals("无持续风向") ? "" : wind.dir,
                                    wind.sc.Equals("微风") ? "很微弱" : wind.sc + "级");
                }

                else if (languange.LanguageTag.Contains("en"))
                {
                    return string.Format("{0}.... The temperature will be about {1} to {2}℉.... Wind {3}..... at {4} km/h",
                                    cond.code_d == cond.code_n ? cond.txt_d : new StringBuilder().Append(cond.txt_d).Append(" to ").Append(cond.txt_n).ToString(),
                                    (int)(tmp.min * 1.8 + 32),
                                    (int)(tmp.max * 1.8 + 32),
                                    wind.dir,
                                    wind.spd);
                }

                return null;
            }
        }
    }
}