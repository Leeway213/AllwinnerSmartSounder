using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;

namespace UWPHelpers
{
    /// <summary>
    /// 使用.NET中的Json API实现的Json帮助类
    /// </summary>
    public static class JsonHelper
    {
        /// <summary>
        /// 将Json对象序列化为json字符串
        /// </summary>
        /// <typeparam name="T">json对象的类型</typeparam>
        /// <param name="jsonObj"></param>
        /// <returns></returns>
        public static string ToJson<T>(T jsonObj)
        {
            var serializer = new DataContractJsonSerializer(typeof(T));
            using (MemoryStream ms = new MemoryStream())
            {
                serializer.WriteObject(ms, jsonObj);
                var jsonArray = ms.ToArray();
                return Encoding.UTF8.GetString(jsonArray);
            }
        }

        /// <summary>
        /// 将json字符串反序列化为json对象
        /// </summary>
        /// <typeparam name="T">json对象的类型</typeparam>
        /// <param name="json"></param>
        /// <returns></returns>
        public static T FromJson<T>(string json)
        {
            var deserializer = new DataContractJsonSerializer(typeof(T));

            try
            {
                using (MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes(json)))
                {
                    return (T)deserializer.ReadObject(ms);
                }
            }
            catch(SerializationException ex)
            {
                throw new SerializationException("Unable to deserialize JSON: " + json, ex);
            }
        }
    }
}
