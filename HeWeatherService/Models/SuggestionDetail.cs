using System.Runtime.Serialization;

namespace HeWeatherService.Models
{
    [DataContract]
    public class SuggestionDetail
    {
        [DataMember]
        public string brf { get; set; }

        [DataMember]
        public string txt { get; set; }
    }
}