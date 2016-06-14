using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage.Streams;

namespace HttpClientHelper
{
    interface IFileDownloadHttpClient
    {
        Task<IRandomAccessStream> GetBinFile(Uri uri);
    }
}
