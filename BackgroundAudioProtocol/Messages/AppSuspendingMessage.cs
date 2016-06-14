using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace BackgroundAudioProtocol.Messages
{
    /// <summary>
    /// APP前台挂起时发送给后台任务的消息实体
    /// </summary>
    [DataContract]
    public class AppSuspendingMessage
    {
        [DataMember]
        public DateTime Timestamp { get; set; }

        public AppSuspendingMessage()
        {
            this.Timestamp = DateTime.Now;
        }

        public AppSuspendingMessage(DateTime timestamp)
        {
            this.Timestamp = timestamp;
        }
    }
}
