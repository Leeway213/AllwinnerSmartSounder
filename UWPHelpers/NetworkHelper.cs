using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Windows.Data.Json;
using Windows.Networking.Connectivity;

namespace UWPHelpers
{
    public class NetworkHelper
    {
        /// <summary>
        /// 检测设备是否可以访问网络
        /// </summary>
        /// <returns></returns>
        public static bool CheckNetworkAvailability()
        {
            var profile = NetworkInformation.GetInternetConnectionProfile();
            if (profile != null)
            {
                var level = profile.GetNetworkConnectivityLevel();
                switch (level)
                {
                    case NetworkConnectivityLevel.InternetAccess:
                        return true;
                    default:
                        return false;
                }
            }
            return false;
        }

        /// <summary>
        /// 获取设备的公网IP(需要设备已连接互联网)。
        /// </summary>
        /// <returns></returns>
        public async static Task<string> GetIPAddress()
        {
            WebRequest request = WebRequest.Create("http://pv.sohu.com/cityjson?ie=utf-8");

            var response = await request.GetResponseAsync();
            using (Stream s = response.GetResponseStream())
            {
                StreamReader reader = new StreamReader(s);
                string html = await reader.ReadToEndAsync();

                JsonObject jobj;

                try
                {
                    jobj = JsonObject.Parse(html.Substring(19, html.Length - 20));
                    IJsonValue jsonValue;
                    jobj.TryGetValue("cip", out jsonValue);
                    return jsonValue.GetString();
                }
                catch (System.Exception ex)
                {
                    throw;
                }
                
            }

        }
    }
}
