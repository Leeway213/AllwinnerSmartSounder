using System.Runtime.Serialization;

namespace HeWeatherService.Models
{
    [DataContract]
    public class Suggestion
    {
        [DataMember]
        public SuggestionDetail comf { get; set; }

        [DataMember]
        public SuggestionDetail cw { get; set; }

        [DataMember]
        public SuggestionDetail drsg { get; set; }

        [DataMember]
        public SuggestionDetail flu { get; set; }

        [DataMember]
        public SuggestionDetail sport { get; set; }

        [DataMember]
        public SuggestionDetail trav { get; set; }

        [DataMember]
        public SuggestionDetail uv { get; set; }
    }
}