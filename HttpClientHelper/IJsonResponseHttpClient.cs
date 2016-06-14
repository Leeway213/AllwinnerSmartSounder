using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HttpClientHelper
{
    interface IJsonResponseHttpClient
    {
        Task<T> GetAsync<T>(Uri uri);
        Task<T> PostAsync<T>(Uri uri, object body);
        Task<T> PutAsync<T>(Uri uri, object body);
        Task<T> DeleteAsync<T>(Uri uri);
    }
}
