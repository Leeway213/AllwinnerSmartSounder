using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.Globalization;
using Windows.Media.SpeechSynthesis;
using Windows.Storage;

namespace SmartSounder.Tools
{
    /// <summary>
    /// 语音合成帮助类
    /// </summary>
    public class SpeechSynthesisHelper
    {

        /// <summary>
        /// 文本合成语音(使用SSML协议标准)
        /// </summary>
        /// <param name="text">要处理的文本</param>
        /// <returns>语音合成流</returns>
        public async static Task<SpeechSynthesisStream> TextToSpeechAsync(string text)
        {
            SpeechSynthesizer speech = new SpeechSynthesizer();
            var voices = SpeechSynthesizer.AllVoices;
            VoiceInformation voice = null;

            //判定文本内容，针对不同的文本内容使用不同的SSML模板（后续添加）
            var file = await StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///Assets/XML/SSML_Weather.xml"));
            using (var stream = await file.OpenReadAsync())
            {
                StreamReader reader = new StreamReader(stream.AsStreamForRead());
                string str = reader.ReadToEnd();
                string result = "";
                switch (AppSettingsConstants.CurrentLanguageType)
                {
                    case LanguageType.Chinese:
                        result = string.Format(str, "zh-CN", text);
                        voice = voices.FirstOrDefault(p => p.Gender == VoiceGender.Female && new Language(p.Language).LanguageTag.Contains("zh"));
                        break;
                    case LanguageType.English:
                        result = string.Format(str, "en-US", text);
                        voice = voices.FirstOrDefault(p => p.Gender == VoiceGender.Female && new Language(p.Language).LanguageTag.Contains("en"));
                        break;
                }
                speech.Voice = voice;
                return await speech.SynthesizeSsmlToStreamAsync(result);
            }
            
        }
    }
}
