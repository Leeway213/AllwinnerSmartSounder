using System.Collections.Generic;
using BackgroundAudioProtocol.Models;

namespace SmartSounder.Tools
{
    public class PlaylistChangedEventArgs
    {
        public List<SongModel> NewList { get; internal set; }
    }
}