using System.Runtime.Serialization;

namespace BackgroundAudioProtocol.Messages
{
    /// <summary>
    /// 后台任务启动时，发送给前台任务的消息实体
    /// </summary>
    [DataContract]
    public class BackgroundAudioTaskStartedMessage
    {
    }
}
