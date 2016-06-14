using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace TulingRobotService.Models
{
    [DataContract]
    public class Response
    {
        [DataMember]
        public string code { get; set; }

        [DataMember]
        public string text { get; set; }

        [DataMember]
        public string url { get; set; }

        [DataMember]
        public List<Detail> list { get; set; }
    }
}
