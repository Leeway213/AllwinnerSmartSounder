using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace BackgroundAudioProtocol.Messages
{
    /// <summary>
    /// App前台恢复时发送给后台任务的消息实体
    /// </summary>
    [DataContract]
    public class AppResumedMessage
    {

        [DataMember]
        public DateTime Timestamp { get; set; }
        
        public AppResumedMessage()
        {
            this.Timestamp = DateTime.Now;
        }

        public AppResumedMessage(DateTime timestamp)
        {
            this.Timestamp = timestamp;
        }
    }
}
