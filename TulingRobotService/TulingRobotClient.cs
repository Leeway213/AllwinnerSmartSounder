using HttpClientHelper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using TulingRobotService.Models;
using UWPHelpers;
using Windows.Web.Http;

namespace TulingRobotService
{
    /// <summary>
    /// 图灵聊天机器人的Client
    /// </summary>
    public class TulingRobotClient : AbsHttpClient
    {
        private static TulingRobotClient _currentClient;

        public static TulingRobotClient CurrentClient
        {
            get
            {
                if (_currentClient == null)
                {
                    _currentClient = new TulingRobotClient(Constants.BASE_URL);
                }
                return _currentClient;
            }
        }

        public Uri Uri { get; set; }

        private RequestData _requestData;
        public RequestData RequestData
        {
            get
            {
                if (_requestData == null)
                {
                    _requestData = new RequestData()
                    {
                        key = Constants.KEY,
                        userid = Constants.USER_ID
                    };
                }
                return _requestData;
            }
        }

        /// <summary>
        /// 获取对话回复内容
        /// </summary>
        /// <param name="info">对话提问</param>
        /// <returns>对话应答</returns>
        public async Task<Response> GetResponse(string info)
        {
            RequestData.info = info;
            var bytes = await PostAsync(Uri, RequestData);
            string jsonStr = Encoding.UTF8.GetString(bytes);
            return JsonHelper.FromJson<Response>(jsonStr);
        }

        private TulingRobotClient(string baseUrl)
        {
            Uri = new Uri(baseUrl, UriKind.RelativeOrAbsolute);
            _httpClient.DefaultRequestHeaders.Accept.TryParseAdd("*/*");
            _httpClient.DefaultRequestHeaders.UserAgent.TryParseAdd("Mozilla/5.0 (Windows NT 10.0) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/50.0.2661.102 Safari/537.36");
            _httpClient.DefaultRequestHeaders.AcceptLanguage.TryParseAdd("zh-CN,zh;q=0.8");
            _httpClient.DefaultRequestHeaders.Connection.TryParseAdd("keep-alive");
        }
    }
}
