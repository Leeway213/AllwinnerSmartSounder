using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Media.SpeechRecognition;

namespace SmartSounder.Tools
{
    public enum LanguageType
    {
        Unknown, Chinese, English
    }
    public class AppSettingsConstants
    {
        public const string LANGUAGE_SETTING = "languange";

        public static string CurrentLanguage
        {
            get
            {
                if (AppSettings.GetValue(LANGUAGE_SETTING) == null)
                {
                    var language = SpeechRecognizer.SystemSpeechLanguage.LanguageTag;
                    AppSettings.SetValue(LANGUAGE_SETTING, language);
                }
                return AppSettings.GetValue(LANGUAGE_SETTING) as string;
            }
        }

        public static LanguageType CurrentLanguageType
        {
            get
            {
                var tag = CurrentLanguage;
                if (tag.Contains("zh"))
                    return LanguageType.Chinese;
                else if (tag.Contains("en"))
                    return LanguageType.English;
                else
                    return LanguageType.Unknown;
            }
        }
    }
}
