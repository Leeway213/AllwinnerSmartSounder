using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Networking;
using Windows.Storage.Streams;

namespace UDPService
{
    public class UDPClient : UDPBase
    {
        private static UDPClient _current;
        public static UDPClient Current
        {
            get
            {
                if (_current == null)
                {
                    _current = new UDPClient();
                }
                return _current;
            }
        }
        public static async void SendData(string IP, string remotePort, byte[] data)
        {
            HostName host = new HostName(IP);
            using (var outputStream = await Current._socket.GetOutputStreamAsync(host, remotePort))
            {
                DataWriter writer = new DataWriter(outputStream);
                writer.WriteBytes(data);
                await writer.StoreAsync();
                writer.Dispose();
            }
        }
    }
}
