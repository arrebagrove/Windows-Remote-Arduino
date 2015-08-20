#if MF_FRAMEWORK_VERSION_V4_3
using Microsoft.SPOT.Net.NetworkInformation;
using Microsoft.SPOT.Time;
using System.Threading;
using System.Net;
#else
using System.Threading.Tasks;
using Windows.Networking.Connectivity;
using Windows.Networking;
#endif
using System;
using System.Text;
using System.Linq;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;

namespace Glovebox.IoT
{
    public static class Util
    {

        const string ntpServer = "au.pool.ntp.org";
        private readonly static string[] postcodes = new string[] { "3000", "6000", "2011" };
        private static Random rnd = new Random(Environment.TickCount);
        const int networkSettleTime = 1000;
        public static TimeSpan utcOffset;

        public static async void GetTime()
        {
            DatagramSocket socket = new DatagramSocket();
            socket.MessageReceived += socket_MessageReceived;
            await socket.ConnectAsync(new HostName("time.windows.com"), "123");

            using (DataWriter writer = new DataWriter(socket.OutputStream))
            {
                byte[] container = new byte[48];
                container[0] = 0x1B;

                writer.WriteBytes(container);
                await writer.StoreAsync();
            }

        }

        static void socket_MessageReceived(DatagramSocket sender, DatagramSocketMessageReceivedEventArgs args)
        {
            using (DataReader reader = args.GetDataReader())
            {
                byte[] b = new byte[48];

                reader.ReadBytes(b);

                DateTime time = Util.GetNetworkTime(b);
                utcOffset = DateTime.UtcNow.Subtract(time);
            }
        }

        public static DateTime GetNetworkTime(byte[] rawData)
        {
            //Offset to get to the "Transmit Timestamp" field (time at which the reply 
            //departed the server for the client, in 64-bit timestamp format."
            const byte serverReplyTime = 40;

            //Get the seconds part
            ulong intPart = BitConverter.ToUInt32(rawData, serverReplyTime);

            //Get the seconds fraction
            ulong fractPart = BitConverter.ToUInt32(rawData, serverReplyTime + 4);

            //Convert From big-endian to little-endian
            intPart = SwapEndianness(intPart);
            fractPart = SwapEndianness(fractPart);

            var milliseconds = (intPart * 1000) + ((fractPart * 1000) / 0x100000000L);

            //**UTC** time
            var networkDateTime = (new DateTime(1900, 1, 1)).AddMilliseconds((long)milliseconds);

            return networkDateTime;
        }

        // stackoverflow.com/a/3294698/162671
        static uint SwapEndianness(ulong x)
        {
            return (uint)(((x & 0x000000ff) << 24) +
                           ((x & 0x0000ff00) << 8) +
                           ((x & 0x00ff0000) >> 8) +
                           ((x & 0xff000000) >> 24));
        }



        public static int RandomNumber(int Range)
        {
            return rnd.Next(Range);
        }

        public static bool SetTime(bool connected)
        {
#if MF_FRAMEWORK_VERSION_V4_3
            if (!connected) { return false; }
            try {
                //network settle time
                Util.Delay(networkSettleTime);
                var ntpAddress = GetTimeServiceAddress(ntpServer);
                if (ntpAddress == null) { return false; }

                TimeService.UpdateNow(ntpAddress, 200);

                return true;
            }
            catch { return false; }
#else
            return true;
#endif
        }

#if MF_FRAMEWORK_VERSION_V4_3
        private static byte[] GetTimeServiceAddress(string TimeServerAddress) {
            try {
                IPAddress[] address = Dns.GetHostEntry(TimeServerAddress).AddressList;
                if (address != null && address.Length > 0) {
                    return address[0].GetAddressBytes();
                }
                return null;
            }
            catch { return null; }
        }
#endif

        public static string GetHostName()
        {
            var hostNamesList = Windows.Networking.Connectivity.NetworkInformation
                .GetHostNames();

            foreach (var entry in hostNamesList)
            {
                if (entry.Type == Windows.Networking.HostNameType.DomainName)
                {
                    return entry.CanonicalName.Split('.')[0];
                }
            }

            return null;
        }


        public static string RandomPostcode()
        {
            return postcodes[rnd.Next(postcodes.Length)];
        }

        public static string BytesToString(byte[] Input)
        {
            char[] Output = new char[Input.Length];
            for (int Counter = 0; Counter < Input.Length; ++Counter)
            {
                Output[Counter] = (char)Input[Counter];
            }
            return new string(Output);
        }

        /// <summary>
        /// convert string to byte array
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static byte[] StringToByteArray(string str)
        {
            return new UTF8Encoding().GetBytes(str);
        }

        public static void SetName(string networkId)
        {
            ConfigurationManager.NetworkId = networkId;
            ConfigurationManager.Location = networkId;
        }

        static public IServiceManager StartNetworkServices(bool connected)
        {
            ConfigurationManager.DeviceId = GetHostName();

            if (!connected) { return null; }

            Util.SetTime(connected);

            if (ConfigurationManager.cloudMode == ConfigurationManager.Mode.MQTT_Maker)
            {
                return new ServiceManagerMqtt(ConfigurationManager.Broker, connected);
            }
            else
            {
                return new ServiceManagerEventBus();
            }
        }

        public static string GetUniqueDeviceId()
        {
#if MF_FRAMEWORK_VERSION_V4_3
            foreach (var nic in NetworkInterface.GetAllNetworkInterfaces()) {
                if (nic.NetworkInterfaceType == NetworkInterfaceType.Ethernet) {
                    return MacToString(nic.PhysicalAddress);
                }
            }
            return string.Empty;
#else
            return Windows.Networking.Connectivity.NetworkInformation.GetInternetConnectionProfile().NetworkAdapter.NetworkAdapterId.ToString();
#endif            
        }

        private static string MacToString(byte[] macAddress)
        {
            string result = string.Empty;
            foreach (var part in macAddress)
            {
                result += part.ToString("X") + "-";
            }
            return result.Substring(0, result.Length - 1);
        }

        public static void Delay(int milliseconds)
        {
#if MF_FRAMEWORK_VERSION_V4_3
            Thread.Sleep(milliseconds);
#else
            Task.Delay(milliseconds).Wait();
#endif
        }

        public static string GetIPAddress()
        {
            string localIP = "?";
#if MF_FRAMEWORK_VERSION_V4_3

            IPHostEntry host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (IPAddress ip in host.AddressList) {
                if (ip.AddressFamily == AddressFamily.InterNetwork) {
                    localIP = ip.ToString();
                }
            }

#else
            HostName localHostName = NetworkInformation.GetHostNames().FirstOrDefault(h =>
                    h.IPInformation != null &&
                    h.IPInformation.NetworkAdapter != null);

            localIP = localHostName.RawName;

#endif
            return localIP;
        }
    }
}
