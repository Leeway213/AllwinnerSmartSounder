using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Networking.Sockets;

namespace UDPService
{
    public abstract class UDPBase
    {
        protected DatagramSocket _socket;

        protected UDPBase()
        {
            _socket = new DatagramSocket();
        }

        protected async void BindPort(string port)
        {
            if (_socket != null)
            {
                _socket.MessageReceived += _socket_MessageReceived;
                await _socket.BindServiceNameAsync(port);
                return;
            }
            throw new Exception("DatagramSocket object need to be instantiated at first");
        }

        protected virtual void _socket_MessageReceived(DatagramSocket sender, DatagramSocketMessageReceivedEventArgs args)
        {

        }
    }
}
