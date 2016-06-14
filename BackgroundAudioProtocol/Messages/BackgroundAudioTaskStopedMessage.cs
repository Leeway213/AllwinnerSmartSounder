using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace BackgroundAudioProtocol.Messages
{
    /// <summary>
    /// 后台任务停止时，发送给前台任务的消息实体
    /// </summary>
    [DataContract]
    public class BackgroundAudioTaskStopedMessage
    {
    }
}
