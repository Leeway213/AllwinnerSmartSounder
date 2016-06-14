using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace BackgroundAudioProtocol.BackgroundAudioSettings
{
    /// <summary>
    /// 保存后台音频中需要的AppSettings的帮助类
    /// </summary>
    public static class BackgroundAudioSettingsHelper
    {
        private static object o = new object();
        public static ApplicationDataContainer Current
        {
            get
            {
                return ApplicationData.Current.LocalSettings;
            }
        }

        public static void SetValue(string key, object value)
        {
            lock (o)
            {
                if (Current.Values.ContainsKey(key))
                {
                    Current.Values[key] = value;
                }
                else
                {
                    Current.Values.Add(new KeyValuePair<string, object>(key, value));
                }
            }

        }

        public static object GetValue(string key)
        {
            if (Current.Values.ContainsKey(key))
            {
                return Current.Values[key];
            }
            return null;
        }

        public static bool DeleteValue(string key)
        {
            if (Current.Values.ContainsKey(key))
            {
                Current.Values.Remove(key);
                return true;
            }

            return false;
        }

    }
}
