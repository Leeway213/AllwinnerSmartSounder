using HeWeatherService.Models;
using HttpClientHelper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UWPHelpers;
using Windows.Data.Json;

namespace HeWeatherService
{
    /// <summary>
    /// 和风天气Client，单例模式
    /// </summary>
    public class HeWeatherClient : AbsHttpClient
    {
        private static HeWeatherClient _currentClient;

        /// <summary>
        /// 当前Client实例
        /// </summary>
        public static HeWeatherClient CurrentClient
        {
            get
            {
                if (_currentClient == null)
                {
                    _currentClient = new HeWeatherClient(Constants.BASE_URL);
                }
                return _currentClient;
            }
        }

        public string BaseUrl { get; set; }

        private HeWeatherClient(string baseAddr) : base()
        {
            this.BaseUrl = baseAddr;

            _httpClient.DefaultRequestHeaders.Accept.TryParseAdd("text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8");
            _httpClient.DefaultRequestHeaders.UserAgent.TryParseAdd("Mozilla/5.0 (Windows NT 10.0) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/50.0.2661.102 Safari/537.36");
            _httpClient.DefaultRequestHeaders.AcceptEncoding.TryParseAdd("gzip, deflate, sdch");
            _httpClient.DefaultRequestHeaders.AcceptLanguage.TryParseAdd("zh-CN,zh;q=0.8");
            _httpClient.DefaultRequestHeaders.Connection.TryParseAdd("keep-alive");
        }


        /// <summary>
        /// 获取天气信息
        /// </summary>
        /// <param name="cityName">城市名称</param>
        /// <returns>天气信息</returns>
        public async Task<HeWeatherResponse> GetWeatherResponse(string cityName)
        {
            Uri uri;
            if (string.IsNullOrEmpty(cityName))
            {
                uri = GetUriFromIP(await NetworkHelper.GetIPAddress());
            }
            else
                uri = GetUri(cityName);
            string responseStr = await GetStringAsync(uri);

            JsonObject jObj;
            if (JsonObject.TryParse(responseStr, out jObj))
            {
                var js = jObj.Values.First().GetArray();
                if (js != null && js.Count > 0)
                {
                    string jsonContent = js[0].ToString();
                    var response = JsonHelper.FromJson<HeWeatherResponse>(jsonContent);
                    return response;
                }
            }

            return null;
        }

        /// <summary>
        /// 获取根据IP查询天气的URI
        /// </summary>
        /// <param name="ip">当前设备的公网IP地址</param>
        /// <returns></returns>
        public Uri GetUriFromIP(string ip)
        {
            Uri uri;
            StringBuilder url = new StringBuilder();
            url.Append(BaseUrl);
            url.Append(BaseUrl.Contains("?") ?
                "&" : "?");

            url.Append("cityip=").Append(ip).
                Append("&key=").Append(Constants.API_KEY);

            uri = new Uri(url.ToString(), UriKind.Absolute);

            return uri;
        }

        /// <summary>
        /// 获取更具城市名称查询天气的URI
        /// </summary>
        /// <param name="city">城市名称</param>
        /// <returns></returns>
        public Uri GetUri(string city)
        {
            Uri uri;
            StringBuilder url = new StringBuilder();
            url.Append(BaseUrl);
            url.Append(BaseUrl.Contains("?") ?
                "&" : "?");

            url.Append("city=").Append(city).
                Append("&key=").Append(Constants.API_KEY);

            uri = new Uri(url.ToString(), UriKind.Absolute);

            return uri;
        }

    }
}
