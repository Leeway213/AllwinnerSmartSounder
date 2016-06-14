
using System;
using System.Threading.Tasks;

namespace HttpClientHelper
{
    interface IBytesArrayResponseHttpClient
    {
        Task<byte[]> GetAsync(Uri uri);
        Task<byte[]> PostAsync(Uri uri, object body);
        Task<byte[]> PutAsync(Uri uri, object body);
        Task<byte[]> DeleteAsync(Uri uri);
    }
}
