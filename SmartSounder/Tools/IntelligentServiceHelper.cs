using IntelligentService;
using IntelligentService.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Globalization;

namespace SmartSounder.Tools
{
    /// <summary>
    /// 命令意图的类型
    /// </summary>
    public enum IntentType
    {
        None, AddToFavorite, SkipBack, Pause, PlayMusic, Goodbye, Resume,
        SkipForward, AlarmOther,
        DeleteAlarm,
        FindAlarm,
        SetAlarm,
        SnoozeAlarm,
        AlarmTimeRemaining,
        TurnOffAlarm,
        ChangeCalendarEntry,
        ChechAvailability,
        ConnectToMeeting,
        CreateCalendarEntry,
        DeleteCalendarEntry,
        FindCalendarEntry,
        FindCalendarWhen,
        FindCalendarWhere,
        FindCalendarWho,
        FindCalendarWhy,
        FindCalendarDuration,
        CalendarTimeRemaining,
        AddContact,
        AnswerPhone,
        AssignNickName,
        CallVoiceMail,
        FindContact,
        ForwardingOff,
        ForwardingOn,
        IgnoreIncoming,
        IgnoreWithMessage,
        MakeCall,
        PressDialKey,
        ReadMessage,
        Redial,
        SpeakerphoneOff,
        SendMessage,
        SpeakerphoneOn,
        FindAttachment,
        FindMyStuff,
        SearchMessages,
        TransformMyStuff,
        OpenSetting,
        RecognizeSong,
        RepeatMusic,
        TurnOffSetting,
        TurnOnSetting,
        ChangeReminder,
        CreateSingleReminder,
        DeleteReminder,
        FindReminder,
        ReadReminder,
        SnoozeReminder,
        TurnOffReminder,
        ChangeTemperatureUnit,
        CheckWeather,
        CheckWeatherFacts,
        CompareWeather,
        GetFrequentLocations,
        GetWeatherAdvisory,
        GetWeatherMaps,
        QuestionWeather,
        ShowWeatherProgression,
        CheckAirQuality,
    }

    public enum CortanaIntentType
    {
        Unknown,
        AlarmOther,
        DeleteAlarm,
        FindAlarm,
        SetAlarm,
        SnoozeAlarm,
        AlarmTimeRemaining,
        TurnOffAlarm,
        ChangeCalendarEntry,
        ChechAvailability,
        ConnectToMeeting,
        CreateCalendarEntry,
        DeleteCalendarEntry,
        FindCalendarEntry,
        FindCalendarWhen,
        FindCalendarWhere,
        FindCalendarWho,
        FindCalendarWhy,
        FindCalendarDuration,
        CalendarTimeRemaining,
        AddContact,
        AnswerPhone,
        AssignNickName,
        CallVoiceMail,
        FindContact,
        ForwardingOff,
        ForwardingOn,
        IgnoreIncoming,
        IgnoreWithMessage,
        MakeCall,
        PressDialKey,
        ReadMessage,
        Redial,
        SpeakerphoneOff,
        SendMessage,
        SpeakerphoneOn,
        FindAttachment,
        FindMyStuff,
        SearchMessages,
        TransformMyStuff,
        OpenSetting,
        PauseMusic,
        PlayMusic,
        RecognizeSong,
        RepeatMusic,
        ResumeMusic,
        SkipBack,
        SkipForward,
        TurnOffSetting,
        TurnOnSetting,
        ChangeReminder,
        CreateSingleReminder,
        DeleteReminder,
        FindReminder,
        ReadReminder,
        SnoozeReminder,
        TurnOffReminder,
        ChangeTemperatureUnit,
        CheckWeather,
        CheckWeatherFacts,
        CompareWeather,
        GetFrequentLocations,
        GetWeatherAdvisory,
        GetWeatherMaps,
        QuestionWeather,
        ShowWeatherProgression,
        CheckAirQuality,
        None
    }

    /// <summary>
    /// LUIS语义理解的帮助类
    /// </summary>
    public class IntelligentServiceHelper
    {
        /// <summary>
        /// 根据提供的短语/句子返回包含意图、意图实体等信息的实例(使用自建的LUIS服务)
        /// </summary>
        /// <param name="utterance">短语/句子</param>
        /// <returns>包含意图、意图实体等信息的响应实例</returns>
        public static async Task<QueryResponse> GetResponse(string utterance)
        {
            Uri uri = null;
            switch (AppSettingsConstants.CurrentLanguageType)
            {
                case LanguageType.Chinese:
                    uri = IntelligentClient.GetUri(Constants.CN_APP_ID, utterance);
                    break;
                case LanguageType.English:
                    uri = IntelligentClient.GetUri(Constants.EN_APP_ID, utterance);
                    //uri = IntelligentClient.GetUri(Constants.EN_CORTANA_APP_ID, utterance);
                    break;
            }
            if (uri != null)
            {
                var response = await IntelligentClient.CurrentClient.GetAsync<QueryResponse>(uri);

                return response;
            }
            return null;
        }

        /// <summary>
        /// 根据提供的短语/句子返回包含意图、意图实体等信息的实例(使用pre-built Cortana LUIS服务)
        /// </summary>
        /// <param name="utterance">短语/句子</param>
        /// <returns>包含意图、意图实体等信息的响应实例</returns>
        public static async Task<QueryResponse> GetCortanaModelResponse(string utterance)
        {
            Uri uri = null;

            switch (AppSettingsConstants.CurrentLanguageType)
            {
                case LanguageType.Unknown:
                    break;
                case LanguageType.Chinese:
                    uri = IntelligentClient.GetUri(Constants.CN_CORTANA_APP_ID, utterance);
                    break;
                case LanguageType.English:
                    uri = IntelligentClient.GetUri(Constants.EN_CORTANA_APP_ID, utterance);
                    break;
                default:
                    break;
            }

            if (uri != null)
            {
                var response = await IntelligentClient.CurrentClient.GetAsync<QueryResponse>(uri);

                return response;
            }
            return null;
        }

        /// <summary>
        /// 根据意图实例获取意图类型
        /// </summary>
        /// <param name="intent"></param>
        /// <returns></returns>
        public static IntentType GetIntentType(Intent intent)
        {
            if (intent == null)
            {
                return IntentType.None;
            }
            switch (intent.intent)
            {
                case "AddToFavorite":
                    return IntentType.AddToFavorite;
                case "sounder.intent.media.skip_back":
                case "builtin.intent.ondevice.skip_back":
                    return IntentType.SkipBack;
                case "sounder.intent.media.play_music":
                case "builtin.intent.ondevice.play_music":
                    return IntentType.PlayMusic;
                case "sounder.intent.media.pause":
                case "builtin.intent.ondevice.pause":
                    return IntentType.Pause;
                case "Goodbye":
                    return IntentType.Goodbye;
                case "sounder.intent.media.resume":
                case "builtin.intent.ondevice.resume":
                    return IntentType.Resume;
                case "sounder.intent.media.skip_forward":
                case "builtin.intent.ondevice.skip_forward":
                    return IntentType.SkipForward;
                case "builtin.intent.alarm.alarm_other":
                    return IntentType.AlarmOther;
                case "builtin.intent.alarm.delete_alarm":
                    return IntentType.DeleteAlarm;
                case "builtin.intent.alarm.find_alarm":
                    return IntentType.FindAlarm;
                case "builtin.intent.alarm.set_alarm":
                    return IntentType.SetAlarm;
                case "builtin.intent.alarm.snooze":
                    return IntentType.SnoozeAlarm;
                case "builtin.intent.alarm.time_remaining":
                    return IntentType.AlarmTimeRemaining;
                case "builtin.intent.alarm.turn_off_alarm":
                    return IntentType.TurnOffAlarm;
                case "builtin.intent.ondevice.turn_off_setting":
                    return IntentType.TurnOffSetting;
                case "builtin.intent.ondevice.turn_on_setting":
                    return IntentType.TurnOnSetting;
                case "builtin.intent.weather.change_temperature_unit":
                    return IntentType.ChangeTemperatureUnit;
                case "builtin.intent.weather.check_weather":
                    return IntentType.CheckWeather;
                case "builtin.intent.weather.check_weather_facts":
                    return IntentType.CheckWeatherFacts;
                case "builtin.intent.weather.compare_weather":
                    return IntentType.CompareWeather;
                case "builtin.intent.weather.get_frequent_locations":
                    return IntentType.GetFrequentLocations;
                case "builtin.intent.weather.get_weather_advisory":
                    return IntentType.GetWeatherAdvisory;
                case "builtin.intent.weather.get_weather_maps":
                    return IntentType.GetWeatherMaps;
                case "builtin.intent.weather.question_weather":
                    return IntentType.QuestionWeather;
                case "builtin.intent.weather.show_weather_progression":
                    return IntentType.ShowWeatherProgression;
                case "builtin.intent.weather.check_air_quality":
                    return IntentType.CheckAirQuality;
                case "None":
                case "builtin.intent.none":
                    return IntentType.None;
                default:
                    return IntentType.None;
            }
        }

        /// <summary>
        /// 暂时不用
        /// </summary>
        /// <param name="intent"></param>
        /// <returns></returns>
        public static CortanaIntentType GetCortanaIntentType(Intent intent)
        {
            if (intent == null)
            {
                return CortanaIntentType.Unknown;
            }
            var intentSplit = intent.intent.Split('.');
            if (intentSplit[0].Equals("builtin"))
            {
                if (intentSplit[1].Equals("intent"))
                {
                    switch (intentSplit[2])
                    {
                        case "alarm":
                            switch (intentSplit[3])
                            {
                                case "alarm_other":
                                    return CortanaIntentType.AlarmOther;
                                case "delete_alarm":
                                    return CortanaIntentType.DeleteAlarm;
                                case "find_alarm":
                                    return CortanaIntentType.FindAlarm;
                                case "set_alarm":
                                    return CortanaIntentType.SetAlarm;
                                case "snooze":
                                    return CortanaIntentType.SnoozeAlarm;
                                case "time_remaining":
                                    return CortanaIntentType.AlarmTimeRemaining;
                                case "turn_off_alarm":
                                    return CortanaIntentType.TurnOffAlarm;
                            }
                            break;
                        case "calendar":
                            switch (intentSplit[3])
                            {
                                case "change_calendar_entry":
                                    return CortanaIntentType.ChangeCalendarEntry;
                                case "check_availability":
                                    return CortanaIntentType.ChechAvailability;
                                case "connect_to_meeting":
                                    return CortanaIntentType.ConnectToMeeting;
                                case "create_calendar_entry":
                                    return CortanaIntentType.CreateCalendarEntry;
                                case "delete_calendar_entry":
                                    return CortanaIntentType.DeleteCalendarEntry;
                                case "find_calendar_entry":
                                    return CortanaIntentType.FindCalendarEntry;
                                case "find_calendar_when":
                                    return CortanaIntentType.FindCalendarWhen;
                                case "find_calendar_where":
                                    return CortanaIntentType.FindCalendarWhere;
                                case "find_calendar_who":
                                    return CortanaIntentType.FindCalendarWho;
                                case "find_calendar_why":
                                    return CortanaIntentType.FindCalendarWhy;
                                case "find_duration":
                                    return CortanaIntentType.FindCalendarDuration;
                                case "time_remaining":
                                    return CortanaIntentType.CalendarTimeRemaining;
                            }
                            break;
                        case "communication":
                            switch (intentSplit[3])
                            {
                                case "add_contact":
                                    return CortanaIntentType.AddContact;
                                case "answer_phone":
                                    return CortanaIntentType.AnswerPhone;
                                case "assign_nickname":
                                    return CortanaIntentType.AssignNickName;
                                case "call_voice_mail":
                                    return CortanaIntentType.CallVoiceMail;
                                case "find_contact":
                                    return CortanaIntentType.FindContact;
                                case "forwarding_off":
                                    return CortanaIntentType.ForwardingOff;
                                case "forwarding_on":
                                    return CortanaIntentType.ForwardingOn;
                                case "ignore_incoming":
                                    return CortanaIntentType.IgnoreIncoming;
                                case "ignore_with_message":
                                    return CortanaIntentType.IgnoreWithMessage;
                                case "make_call":
                                    return CortanaIntentType.MakeCall;
                                case "press_key":
                                    return CortanaIntentType.PressDialKey;
                                case "read_aloud":
                                    return CortanaIntentType.ReadMessage;
                                case "redial":
                                    return CortanaIntentType.Redial;
                                case "send_email":
                                    return CortanaIntentType.SpeakerphoneOff;
                                case "send_text":
                                    return CortanaIntentType.SendMessage;
                                case "speakerphone_off":
                                    return CortanaIntentType.SpeakerphoneOff;
                                case "speakerphone_on":
                                    return CortanaIntentType.SpeakerphoneOn;
                            }
                            break;
                        case "mystuff":
                            switch (intentSplit[3])
                            {
                                case "find_attachment":
                                    return CortanaIntentType.FindAttachment;
                                case "find_my_stuff":
                                    return CortanaIntentType.FindMyStuff;
                                case "search_messages":
                                    return CortanaIntentType.SearchMessages;
                                case "transform_my_stuff":
                                    return CortanaIntentType.TransformMyStuff;
                            }
                            break;
                        case "ondevice":
                            switch (intentSplit[3])
                            {
                                case "open_setting":
                                    return CortanaIntentType.OpenSetting;
                                case "pause":
                                    return CortanaIntentType.PauseMusic;
                                case "play_music":
                                    return CortanaIntentType.PlayMusic;
                                case "recognize_song":
                                    return CortanaIntentType.RecognizeSong;
                                case "repeat":
                                    return CortanaIntentType.RepeatMusic;
                                case "resume":
                                    return CortanaIntentType.ResumeMusic;
                                case "skip_back":
                                    return CortanaIntentType.SkipBack;
                                case "skip_forward":
                                    return CortanaIntentType.SkipForward;
                                case "turn_off_setting":
                                    return CortanaIntentType.TurnOffSetting;
                                case "turn_on_setting":
                                    return CortanaIntentType.TurnOnSetting;
                            }
                            break;
                        case "places":
                            break;
                        case "reminder":
                            switch (intentSplit[3])
                            {
                                case "change_reminder":
                                    return CortanaIntentType.ChangeReminder;
                                case "create_single_reminder":
                                    return CortanaIntentType.CreateSingleReminder;
                                case "delete_reminder":
                                    return CortanaIntentType.DeleteReminder;
                                case "find_reminder":
                                    return CortanaIntentType.FindReminder;
                                case "read_aloud":
                                    return CortanaIntentType.ReadReminder;
                                case "snooze":
                                    return CortanaIntentType.SnoozeReminder;
                                case "turn_off_reminder":
                                    return CortanaIntentType.TurnOffReminder;
                            }
                            break;
                        case "weather":
                            switch (intentSplit[3])
                            {
                                case "change_temperature_unit":
                                    return CortanaIntentType.ChangeTemperatureUnit;
                                case "check_weather":
                                    return CortanaIntentType.CheckWeather;
                                case "check_weather_facts":
                                    return CortanaIntentType.CheckWeatherFacts;
                                case "compare_weather":
                                    return CortanaIntentType.CompareWeather;
                                case "get_frequent_locations":
                                    return CortanaIntentType.GetFrequentLocations;
                                case "get_weather_advisory":
                                    return CortanaIntentType.GetWeatherAdvisory;
                                case "get_weather_maps":
                                    return CortanaIntentType.GetWeatherMaps;
                                case "question_weather":
                                    return CortanaIntentType.QuestionWeather;
                                case "show_weather_progression":
                                    return CortanaIntentType.ShowWeatherProgression;
                            }
                            break;
                        case "none":
                            return CortanaIntentType.None;
                        default:
                            break;
                    }
                }
            }

            return CortanaIntentType.None;
        }
    }
}
