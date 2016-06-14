using HttpClientHelper.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Web.Http;
using System.Threading;
using UWPHelpers;
using System.IO;
using Windows.Storage.Streams;

namespace HttpClientHelper
{
    /// <summary>
    /// 所有Http协议客户端的基类
    /// </summary>
    public abstract class AbsHttpClient : IJsonResponseHttpClient, IBytesArrayResponseHttpClient, IStringResponseHttpClient
    {
        protected HttpClient _httpClient;

        protected CancellationTokenSource _cancelTokenSource;

        protected AbsHttpClient()
        {
            _httpClient = new HttpClient();
            _cancelTokenSource = new CancellationTokenSource();
        }

        /// <summary>
        /// 返回Json对象的DELETE请求
        /// </summary>
        /// <typeparam name="T">Json对象类型</typeparam>
        /// <param name="uri"></param>
        /// <returns></returns>
        public async Task<T> DeleteAsync<T>(Uri uri)
        {
            //if (_httpClient == null)
            //    throw new HttpClientInvalidException();

            //using (var response = await _httpClient.DeleteAsync(uri).AsTask(_cancelTokenSource.Token))
            using (var response = await GetHttpResponseMessage(uri, HttpMethod.Delete))
            {
                if (response.IsSuccessStatusCode)
                {
                    string body = await response.Content.ReadAsStringAsync();

                    try
                    {
                        return JsonHelper.FromJson<T>(body);
                    }
                    catch (Exception ex)
                    {
                        throw ex;
                    }
                }

                throw new Exception(response.ReasonPhrase);

            }
        }

        /// <summary>
        /// 返回Json对象的GET请求
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="uri"></param>
        /// <returns></returns>
        public async Task<T> GetAsync<T>(Uri uri)
        {
            //if (_httpClient == null)
            //    throw new HttpClientInvalidException();

            //using (var response = await _httpClient.GetAsync(uri).AsTask(_cancelTokenSource.Token))
            using (var response = await GetHttpResponseMessage(uri, HttpMethod.Get))
            {
                if (response.IsSuccessStatusCode)
                {
                    string body = await response.Content.ReadAsStringAsync();

                    try
                    {
                        return JsonHelper.FromJson<T>(body);
                    }
                    catch (Exception ex)
                    {
                        throw ex;
                    }
                }

                throw new Exception(response.ReasonPhrase);

            }
        }

        /// <summary>
        /// 返回Json对象的POST请求
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="uri"></param>
        /// <param name="body"></param>
        /// <returns></returns>
        public async Task<T> PostAsync<T>(Uri uri, object body)
        {
            //if (_httpClient == null)
            //{
            //    throw new HttpClientInvalidException();
            //}
            try
            {
                string data = JsonHelper.ToJson(body);

                //using (var response = await _httpClient.PostAsync(uri, new HttpStringContent(data)).AsTask(_cancelTokenSource.Token))
                using (var response = await GetHttpResponseMessage(uri, HttpMethod.Post, data))
                {
                    if (response.IsSuccessStatusCode)
                    {
                        string jsonStr = await response.Content.ReadAsStringAsync();
                        return JsonHelper.FromJson<T>(jsonStr);
                    }

                    throw new Exception(response.ReasonPhrase);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 返回Json对象的PUT请求
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="uri"></param>
        /// <param name="body"></param>
        /// <returns></returns>
        public async Task<T> PutAsync<T>(Uri uri, object body)
        {
            //if (_httpClient == null)
            //{
            //    throw new HttpClientInvalidException();
            //}
            try
            {
                string data = JsonHelper.ToJson(body);

                //using (var response = await _httpClient.PutAsync(uri, new HttpStringContent(data)).AsTask(_cancelTokenSource.Token))
                using (var response = await GetHttpResponseMessage(uri, HttpMethod.Put, data))
                {
                    if (response.IsSuccessStatusCode)
                    {
                        string jsonStr = await response.Content.ReadAsStringAsync();
                        return JsonHelper.FromJson<T>(jsonStr);
                    }

                    throw new Exception(response.ReasonPhrase);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 返回byte数组的POST请求
        /// </summary>
        /// <param name="uri"></param>
        /// <param name="body"></param>
        /// <returns></returns>
        public async Task<byte[]> PostAsync(Uri uri, object body)
        {
            try
            {
                string data = JsonHelper.ToJson(body);
                using (var response = await GetHttpResponseMessage(uri, HttpMethod.Post, data))
                {
                    if (response.IsSuccessStatusCode)
                    {
                        return await GetBytesFromHttpResponse(response);
                    }
                    throw new Exception(response.ReasonPhrase);
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// 返回byte数组的PUT请求
        /// </summary>
        /// <param name="uri"></param>
        /// <param name="body"></param>
        /// <returns></returns>
        public async Task<byte[]> PutAsync(Uri uri, object body)
        {
            try
            {
                string data = JsonHelper.ToJson(body);
                using (var response = await GetHttpResponseMessage(uri, HttpMethod.Put, data))
                {
                    if (response.IsSuccessStatusCode)
                    {
                        return await GetBytesFromHttpResponse(response);
                    }
                    throw new Exception(response.ReasonPhrase);
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// 返回byte数组的DELETE请求
        /// </summary>
        /// <param name="uri"></param>
        /// <returns></returns>
        public async Task<byte[]> DeleteAsync(Uri uri)
        {
            try
            {
                using (var response = await GetHttpResponseMessage(uri, HttpMethod.Delete))
                {
                    if (response.IsSuccessStatusCode)
                    {
                        return await GetBytesFromHttpResponse(response);
                    }
                    throw new Exception(response.ReasonPhrase);
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// 返回byte数组的GET请求
        /// </summary>
        /// <param name="uri"></param>
        /// <returns></returns>
        public async Task<byte[]> GetAsync(Uri uri)
        {
            try
            {
                using (var response = await GetHttpResponseMessage(uri, HttpMethod.Get))
                {
                    if (response.IsSuccessStatusCode)
                    {
                        return await GetBytesFromHttpResponse(response);
                    }
                    throw new Exception(response.ReasonPhrase);
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// 获取HttpResponseMessage
        /// </summary>
        /// <param name="uri"></param>
        /// <param name="method">Http请求类型：GET/POST/DELETE/PUT</param>
        /// <param name="content">请求的内容，GET和DELETE时不使用此参数</param>
        /// <returns></returns>
        public async Task<HttpResponseMessage> GetHttpResponseMessage(Uri uri, HttpMethod method, string content = null)
        {
            if (_httpClient == null)
            {
                throw new HttpClientInvalidException();
            }
            try
            {
                HttpRequestMessage request = new HttpRequestMessage(method, uri);
                if (content != null)
                {
                    request.Content = new HttpStringContent(content);
                }
                var response = await _httpClient.SendRequestAsync(request).AsTask(_cancelTokenSource.Token);
                return response;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 读取HttpResponse的内容到byte数组
        /// </summary>
        /// <param name="response"></param>
        /// <returns></returns>
        public async Task<byte[]> GetBytesFromHttpResponse(HttpResponseMessage response)
        {
            var input = await response.Content.ReadAsInputStreamAsync();
            MemoryStream stream = new MemoryStream();
            await RandomAccessStream.CopyAsync(input, stream.AsOutputStream());
            stream.Seek(0, SeekOrigin.Begin);
            byte[] buffer = new byte[stream.Length];
            int read = 0;
            do
            {
                read = stream.Read(buffer, 0, (int)stream.Length);
            } while (read != (int)stream.Length);

            input.Dispose();
            stream.Dispose();
            return buffer;
        }

        /// <summary>
        /// 执行GET请求，返回字符串内容
        /// </summary>
        /// <param name="uri"></param>
        /// <returns></returns>
        public async Task<string> GetStringAsync(Uri uri)
        {
            using (var response = await GetHttpResponseMessage(uri, HttpMethod.Get))
            {
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadAsStringAsync();
                }

                throw new Exception(response.ReasonPhrase);
            }
        }
    }
}
