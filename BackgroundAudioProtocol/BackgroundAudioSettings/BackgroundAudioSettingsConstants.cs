using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackgroundAudioProtocol.BackgroundAudioSettings
{

    /// <summary>
    /// App前台状态
    /// </summary>
    public enum AppState
    {
        Unknown, Active, Suspended
    }

    /// <summary>
    /// APP后台状态信息
    /// </summary>
    public enum BackgroundTaskState
    {
        Unknown,
        Started,
        Running,
        Canceled
    }

    public class BackgroundAudioSettingsConstants
    {

        public const string APP_STATE = "appstate";

        public const string BACKGROUND_TASK_STATE = "backgroundtaskstate";

        public const string TRACK_INDEX = "trackindex";

        public const string POSITION = "position";

        public const string LANGUAGE = "language";

        public const string RESUME_AGAIN = "resumeagain";

    }
}
