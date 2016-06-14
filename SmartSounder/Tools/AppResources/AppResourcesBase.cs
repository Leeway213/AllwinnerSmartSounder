using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources;

namespace SmartSounder.Tools.AppResources
{
    /// <summary>
    /// 代码中访问所有字符串资源的基类
    /// </summary>
    public class AppResourcesBase
    {
        protected static ResourceLoader _currentResourceLoader
        {
            get
            {
                return _loader ?? (_loader = ResourceLoader.GetForCurrentView());
            }
        }

        private static ResourceLoader _loader;

        protected static Dictionary<string, string> _resourceCache = new Dictionary<string, string>();

        /// <summary>
        /// 根据资源key获取字符串资源
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static string GetString(string key)
        {
            string result;
            if (_resourceCache.TryGetValue(key, out result))
            {
                return result;
            }

            result = _currentResourceLoader.GetString(key);
            _resourceCache[key] = result;
            return result;
        }

    }
}
