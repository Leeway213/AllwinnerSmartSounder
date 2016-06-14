using GalaSoft.MvvmLight.Threading;
using IntelligentService.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UWPHelpers;

namespace SmartSounder.Tools.Weather
{
    public class WeatherHelper
    {
        /// <summary>
        /// 执行查询天气命令
        /// </summary>
        /// <param name="entities">包含查询天气需要的地理位置、时间等实体</param>
        /// <returns>天气播报内容</returns>
        public static async Task<string> CheckWeather(Entity[] entities)
        {
            DateTime dateTime = DateTime.Today;
            string location = null;
            string time = "";
            foreach (var entity in entities)
            {
                switch (entity.type)
                {
                    case "builtin.weather.date_range":
                        if (entity.resolution.resolution_type == null || entity.resolution.resolution_type.Equals("builtin.datetime.date"))
                        {
                            if (entity.resolution.date != null)
                            {
                                dateTime = DateTimeHelper.String2DateTime(entity.resolution.date);
                                time = entity.entity;
                            }
                        }
                        break;
                    case "builtin.weather.absolute_location":
                        location = entity.entity;
                        break;
                }
            }

            var response = await HeWeatherService.HeWeatherClient.CurrentClient.GetWeatherResponse(location);
            if (response.status.Equals("unknown city"))
            {
                string result = "";
                await App.MainFrame.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                {
                    result = string.Format(AppResources.AppResources.city_is_unknown, location);
                });
                return result;
            }
            foreach (var weather in response.daily_forecast)
            {
                if (DateTime.Parse(weather.date).DayOfYear == dateTime.DayOfYear)
                {
                    string result = "";
                    await App.MainFrame.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                    {
                        result = string.Format(AppResources.AppResources.weather_context, time, location, weather.WeatherText);
                    });
                    return result;
                }
            }

            return null;
        }

        /// <summary>
        /// 执行带特殊问题的天气查询命令，如：会不会下雨、需不需要打伞等
        /// </summary>
        /// <param name="entities">包含查询天气时需要的地理位置、时间等实体</param>
        /// <returns>根据提问问题和天气状况返回的建议内容</returns>
        public static async Task<string> CheckWeatherQuestion(Entity[] entities)
        {
            string result = "";
            DateTime dateTime = DateTime.Today;
            string location = null;
            string time = "";
            string suitableValue = null;
            string suitableEntity = null;
            string conditionValue = null;
            string conditionEntity = null;
            foreach (var entity in entities)
            {
                switch (entity.type)
                {
                    case "builtin.weather.date_range":
                        if (entity.resolution.resolution_type == null || entity.resolution.resolution_type.Equals("builtin.datetime.date"))
                        {
                            if (entity.resolution.date != null)
                            {
                                dateTime = DateTimeHelper.String2DateTime(entity.resolution.date);
                                time = entity.entity;
                            }
                        }
                        break;
                    case "builtin.weather.absolute_location":
                        location = entity.entity;
                        break;
                    case "builtin.weather.suitable_for":
                        suitableEntity = entity.entity;
                        if (entity.resolution != null && entity.resolution.resolution_type == "metadataItems")
                        {
                            suitableValue = entity.resolution.value;
                        }
                        break;
                    case "builtin.weather.weather_condition":
                        conditionEntity = entity.entity;
                        if (entity.resolution != null && entity.resolution.resolution_type == "metadataItems")
                        {
                            conditionValue = entity.resolution.value;
                        }
                        break;
                }
            }

            var response = await HeWeatherService.HeWeatherClient.CurrentClient.GetWeatherResponse(location);
            if (response.status.Equals("unknown city"))
            {
                await App.MainFrame.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                {
                    result = string.Format(AppResources.AppResources.city_is_unknown, location);
                });
                return result;
            }
            foreach (var weather in response.daily_forecast)
            {
                if (!string.IsNullOrEmpty(suitableValue))
                {

                }
                if (DateTime.Parse(weather.date).DayOfYear == dateTime.DayOfYear)
                {
                    await App.MainFrame.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                    {
                        result = string.Format(AppResources.AppResources.weather_context, time, location, weather.WeatherText);
                    });
                    return result;
                }
            }
            return null;
        }

        private string SuitableWeather(string suitableValue, string suitableString, HeWeatherService.Models.DailyForecast weather)
        {
            switch (suitableValue)
            {
                case "hot":
                    break;
                case "cold":
                    break;
                case "nice":
                    break;
                case "rain":
                    string umbrella = "";
                    DispatcherHelper.CheckBeginInvokeOnUI(() =>
                    {
                        umbrella = AppResources.AppResources.umbrella;
                    });
                    if (suitableString == umbrella)
                    {
                        if (IsRainy(weather.cond.code_d) || IsRainy(weather.cond.code_n))
                        {

                        }
                    }
                    break;
                case "sun":
                    break;
                default:
                    return null;
            }
            return null;
        }

        private bool IsRainy(int code)
        {
            switch (code)
            {
                case 213:
                case 300:
                case 301:
                case 302:
                case 303:
                case 304:
                case 305:
                case 306:
                case 307:
                case 308:
                case 309:
                case 310:
                case 311:
                case 312:
                case 313:
                case 404:
                case 405:
                case 406:
                    return true;
                default:
                    return false;
            }
        }

    }
}
