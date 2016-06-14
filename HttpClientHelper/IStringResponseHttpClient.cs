using System;
using System.Threading.Tasks;

namespace HttpClientHelper
{
    interface IStringResponseHttpClient
    {
        Task<string> GetStringAsync(Uri uri);
    }
}
