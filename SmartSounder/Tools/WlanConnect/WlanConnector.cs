using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UDPService;
using UDPService.Messages;
using UWPHelpers;
using Windows.Devices.Radios;
using Windows.Devices.WiFi;
using Windows.Devices.WiFiDirect;
using Windows.Foundation;
using Windows.Security.Credentials;
using Windows.Storage.Streams;

namespace SmartSounder.Tools.WlanConnect
{
    public class WlanConnector
    {
        public const string SSID = "Allwinner_FziD";
        public const string PASSWORD = "AllWinner*4A";

        private static WiFiDirectAdvertisementPublisher wifiPublisher;
        private static WiFiDirectConnectionListener wifiConnectionListener;
        private static AutoResetEvent resetEvent;
        public static async Task<bool> TryConnectWlan()
        {
            resetEvent = new AutoResetEvent(false);
            try
            {
                var access = await WiFiAdapter.RequestAccessAsync();
                if (access == WiFiAccessStatus.Allowed)
                {
                    TurnOnWiFi();
                    StartWiFiHotspot();

                    var result = resetEvent.WaitOne(120000);

                    DisposeWiFiPublisher();
                    return result;
                }

                return false;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private static async void TurnOnWiFi()
        {
            var status = await Radio.RequestAccessAsync();
            if (status == RadioAccessStatus.Allowed)
            {
                var radios = await Radio.GetRadiosAsync();
                foreach (var radio in radios)
                {
                    if (radio.Kind == RadioKind.WiFi)
                    {
                        if (radio.State != RadioState.On)
                        {
                            try
                            {
                                await radio.SetStateAsync(RadioState.On);
                                break;
                            }
                            catch (Exception ex)
                            {
                                throw ex;
                            }
                        }
                    }
                }
            }
        }

        private static void StartWiFiHotspot()
        {
            wifiPublisher = new WiFiDirectAdvertisementPublisher();
            wifiConnectionListener = new WiFiDirectConnectionListener();

            wifiConnectionListener.ConnectionRequested += WifiConnectionListener_ConnectionRequested;

            wifiPublisher.Advertisement.IsAutonomousGroupOwnerEnabled = true;
            wifiPublisher.Advertisement.LegacySettings.Ssid = SSID;
            wifiPublisher.Advertisement.LegacySettings.IsEnabled = true;
            wifiPublisher.Advertisement.LegacySettings.Passphrase.Password = PASSWORD;
            wifiPublisher.StatusChanged += WifiPublisher_StatusChanged;
            wifiPublisher.Start();
        }

        private static void WifiConnectionListener_ConnectionRequested(WiFiDirectConnectionListener sender, WiFiDirectConnectionRequestedEventArgs args)
        {
            Debug.WriteLine("ConnRequested: " + args.GetConnectionRequest().DeviceInformation.Id);
            UDPServer.Current.MessageReceived += Current_MessageReceived;
        }

        private static void WifiPublisher_StatusChanged(WiFiDirectAdvertisementPublisher sender, WiFiDirectAdvertisementPublisherStatusChangedEventArgs args)
        {
            Debug.WriteLine("WiFi Hotspot status: " + args.Status.ToString());
        }

        private static void Current_MessageReceived(Windows.Networking.Sockets.DatagramSocket sender, MessageModel message)
        {
            UDPServer.Current.MessageReceived -= Current_MessageReceived;
            try
            {
                switch (message.Type)
                {
                    case MessageType.WiFiConnect:
                        using (DataReader dataReader = DataReader.FromBuffer(message.Content))
                        {
                            dataReader.UnicodeEncoding = Windows.Storage.Streams.UnicodeEncoding.Utf8;
                            string str = dataReader.ReadString(message.ContentLength);
                            string[] s = str.Split('@');
                            string ssid = s[0];
                            string password = s[1];
                            ConnectWlan(ssid, password);
                            resetEvent.Set();
                            //To-Do: 发送消息告诉客户端连接结果
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private static void ConnectWlan(string ssid, string password)
        {
            try
            {
                IAsyncOperation<IReadOnlyList<WiFiAdapter>> operation1 = WiFiAdapter.FindAllAdaptersAsync();
                var adapters = operation1.GetResults();
                var adapter = adapters.FirstOrDefault();
                var ignore = adapter.ScanAsync();
                foreach (var network in adapter.NetworkReport.AvailableNetworks)
                {
                    if (network.Ssid.Equals(ssid))
                    {
                        PasswordCredential passwordCredential = new PasswordCredential();
                        passwordCredential.Password = password;
                        var operation2 = adapter.ConnectAsync(network, WiFiReconnectionKind.Automatic, passwordCredential);
                        operation2.AsTask().Wait();
                        var result = operation2.GetResults();
                        if (result.ConnectionStatus != WiFiConnectionStatus.Success)
                        {
                            throw new Exception("Connect WiFi failed! Reason: " + result.ConnectionStatus.ToString());
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        private static void DisposeWiFiPublisher()
        {
            wifiPublisher.Stop();
            wifiPublisher.StatusChanged -= WifiPublisher_StatusChanged;
            wifiPublisher = null;
            wifiConnectionListener.ConnectionRequested -= WifiConnectionListener_ConnectionRequested;
            wifiConnectionListener = null;
        }
    }
}
