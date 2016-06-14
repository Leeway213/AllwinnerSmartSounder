using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Windows.Networking;

namespace UDPService
{
    [DataContract]
    public class ClientInfo
    {
        [DataMember]
        public string HostName { get; set; }

        [DataMember]
        public string Token { get; set; }

        [DataMember]
        public uint RemotePort { get; set; }
    }
}
