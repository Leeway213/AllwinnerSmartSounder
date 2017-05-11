using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartSounder.Tools.AppResources
{
    /// <summary>
    /// 所有字符串资源的代码访问入口
    /// </summary>
    public class AppResources : AppResourcesBase
    {
        public static string cannot_find_any_music
        {
            get
            {
                return GetString("cannot_find_any_music");
            }
        }

        public static string cannot_find_any_music_requred
        {
            get
            {
                return GetString("cannot_find_any_music_requred");
            }
        }

        public static string request_audio_failed
        {
            get
            {
                return GetString("request_audio_failed");
            }
        }

        public static string city_is_unknown
        {
            get
            {
                return GetString("city_is_unknown");
            }
        }

        public static string weather_context
        {
            get
            {
                return GetString("weather_context");
            }
        }

        public static string cannot_catch_what_you_said
        {
            get
            {
                return GetString("cannot_catch_what_you_said");
            }
        }

        public static string cannot_catch_what_you_want
        {
            get
            {
                return GetString("cannot_catch_what_you_want");
            }
        }

        public static string network_failed
        {
            get
            {
                return GetString("network_failed");
            }
        }

        public static string welcome_back
        {
            get
            {
                return GetString("welcome_back");
            }
        }

        public static string umbrella
        {
            get
            {
                return GetString("umbrella");
            }
        }

        public static string Chinese
        {
            get
            {
                return GetString("Chinese");
            }
        }

        public static string English
        {
            get
            {
                return GetString("English");
            }
        }
    }
}
