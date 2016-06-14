using HttpClientHelper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Web.Http;

namespace IntelligentService
{
    /// <summary>
    /// 微软LUIS客户端
    /// </summary>
    public class IntelligentClient : AbsHttpClient
    {
        private static IntelligentClient _currentClient;
        public static IntelligentClient CurrentClient
        {
            get
            {
                if (_currentClient == null)
                {
                    _currentClient = new IntelligentClient(Constants.BASE_URL);
                }
                return _currentClient;
            }
        }

        public string BaseAddress { get; private set; }

        private IntelligentClient(string baseAddr) : base()
        {

            _httpClient.DefaultRequestHeaders.Accept.TryParseAdd("*/*");
            _httpClient.DefaultRequestHeaders.AcceptEncoding.TryParseAdd("identity");

            BaseAddress = baseAddr;
        }

        /// <summary>
        /// 根据LUIS APP ID和短语/句子生成查询URI
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="queryStr"></param>
        /// <returns></returns>
        public static Uri GetUri(string appId, string queryStr)
        {
            Uri uri;
            StringBuilder url = new StringBuilder();

            url.Append(CurrentClient.BaseAddress);

            url.Append(CurrentClient.BaseAddress.Contains("?") ?
                "&" : "?");

            url.Append("id=").Append(appId).
                Append("&subscription-key=").Append(Constants.SUBSCRIPTION_KEY).
                Append("&q=").Append(queryStr);

            uri = new Uri(url.ToString(), UriKind.Absolute);

            return uri;
        }
    }
}
