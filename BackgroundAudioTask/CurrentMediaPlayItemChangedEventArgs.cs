using BackgroundAudioProtocol.Models;

namespace BackgroundAudioTask
{
    public sealed class CurrentMediaPlayItemChangedEventArgs
    {
        public object NewItem { get; set; }
    }
}