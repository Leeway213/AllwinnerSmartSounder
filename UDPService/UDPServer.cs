using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using UDPService.Messages;
using UWPHelpers;
using Windows.Networking;
using Windows.Networking.Sockets;
using Windows.Security.Cryptography;
using Windows.Storage;
using Windows.Storage.Streams;

namespace UDPService
{

    public class UDPServer : UDPBase
    {
        private static UDPServer _current;

        public static UDPServer Current
        {
            get
            {
                return _current;
            }
        }

        public static void StartService(string port)
        {
            _current = new UDPServer(port);
        }

        public List<ClientInfo> ClientList { get; set; }
        public string Port { get; private set; }

        public delegate void MessageReceivedEventHandler(DatagramSocket sender, MessageModel message);
        public event MessageReceivedEventHandler MessageReceived;


        private object lockObj;

        private UDPServer(string port) : base()
        {
            lockObj = new object();
            BindPort(port);
            Port = port;
            ClientList = new List<ClientInfo>();

            GetClientList();
        }

        private async void GetClientList()
        {
            try
            {
                StorageFile clientListFile = await ApplicationData.Current.LocalFolder.GetFileAsync("clientlist.json");
                using (var stream = await clientListFile.OpenReadAsync())
                {
                    StreamReader reader = new StreamReader(stream.AsStream());
                    string json = reader.ReadToEnd();
                    ClientList = JsonHelper.FromJson<List<ClientInfo>>(json);
                }
            }
            catch { }
        }

        public async void Dispose()
        {
            if (_socket != null)
            {
                await _socket.CancelIOAsync();
                _socket.Dispose();
                _socket = null;
            }
        }

        public async void SaveClientList()
        {
            if (ClientList == null || ClientList?.Count == 0)
            {
                return;
            }
            string json = JsonHelper.ToJson(ClientList);
            StorageFile clientListFile = await ApplicationData.Current.LocalFolder.CreateFileAsync("clientlist.json", CreationCollisionOption.ReplaceExisting);
            using (var stream = await clientListFile.OpenAsync(FileAccessMode.ReadWrite))
            {
                StreamWriter writer = new StreamWriter(stream.AsStream());
                writer.Write(json);
                writer.Flush();
            }
        }

        protected override void _socket_MessageReceived(DatagramSocket sender, DatagramSocketMessageReceivedEventArgs args)
        {
            Debug.WriteLine(args.RemoteAddress.DisplayName);

            using (DataReader reader = args.GetDataReader())
            {
                MessageModel message = MessageModel.ToMessage(reader);
                //byte[] data = new byte[reader.UnconsumedBufferLength];
                //reader.ReadBytes(data);
                //MessageModel message = MessageModel.ToMessageFromEncrypted(data);

                switch (message.Type)
                {
                    case MessageType.DeviceDetection:
                        ResponseDeviceDetection(message, args.RemoteAddress);
                        break;
                }
                MessageReceived?.Invoke(sender, message);
            }

        }

        private void ResponseDeviceDetection(MessageModel request, HostName remoteHost)
        {
            MessageModel response = new MessageModel();
            response.Type = MessageType.DeviceDetection;
            response.RemotePort = request.RemotePort;

            string token = "";
            ClientInfo existClient = null;
            if (ClientList.Count > 0)
            {
                existClient = ClientList.FirstOrDefault(p => p.HostName.Equals(remoteHost.RawName));
            }
            if (existClient != null)
            {
                token = existClient.Token;
            }
            else
            {
                token = GenerateRandomToken(8);

                ClientList.Add(new ClientInfo()
                {
                    HostName = remoteHost.RawName,
                    RemotePort = response.RemotePort,
                    Token = token
                });
            }

            response.Token = token;
            var data = MessageModel.FromMessage(response);
            UDPClient.SendData(remoteHost.CanonicalName, response.RemotePort.ToString(), data);
            Debug.WriteLine("response to " + remoteHost.CanonicalName + " for " + response.Type);
        }

        private string GenerateRandomToken(uint length)
        {
            IBuffer randomTokenBuffer = CryptographicBuffer.GenerateRandom(length);
            using (DataReader reader = DataReader.FromBuffer(randomTokenBuffer))
            {
                byte[] bytes = new byte[randomTokenBuffer.Capacity];
                reader.ReadBytes(bytes);
                return Convert.ToBase64String(bytes);
            }
        }
    }
}
