
namespace Glovebox.IoT {
    public static class ConfigurationManager {
        public enum Mode {
            MQTT_Maker,
            EventHub_Enterprise
        }

        public const Mode cloudMode = Mode.MQTT_Maker;
        public static string Organisation = "Microsoft";
        public static string Location = "Sydney";
        public static string NetworkId { get; set; }
        public static string DeviceId { get; set; }
    //    get { return _devId; }
    //set {
    //            _devId = value == null || value.Length == 0 ? "emul" : value;
    //            _devId = _devId.Length > 5 ? _devId.Substring(0, 5) : _devId;
    //        }
    //    }


        #region Event Hub Config

        public const string ServicebusNamespace = "MakerDen-ns";
        public const string EventHubName = "ehdevices";
        public const string KeyName = "D1";
        public const string Key = "sFhEe2pLQkWuzXW+5dcOmRZ36GrZy/9/io7DijcVhdc=";

        #endregion

        #region MQTT

        // Best efforts to run the MQTT Broker at gloveboxAE.cloudapp.net 
        // You can install your own instance of Mosquitto MQTT Server from http://mosquitto.org 
        public static string Broker = "gloveboxAE.cloudapp.net";
        public static string MqttNameSpace = "gb/";
        public static string[] MqqtSubscribe = new string[] { "gbcmd/#" };
        public static string MqttDeviceAnnounce = "gbdevice/";
        public static uint mqttPrePublishDelay = 250;  // milliseconds delay before mqtt publish
        public static uint mqttPostPublishDelay = 200; // milliseconds delay after mqtt publish

        #endregion



        //private static string _devId = "emul";





    }
}
