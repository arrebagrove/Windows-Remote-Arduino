using Glovebox.IoT.Json;
using System;

namespace Glovebox.IoT.Telemetry {
    public class SensorMakerDen : ISensorTelemetry {

        JSONWriter jw = new JSONWriter();
        public enum ValuesPerSample { One = 1, Two = 2, Three = 3, Four = 4, Five = 5 };

        string DeviceName { get; set; }
        string Type { get; set; }
        string Unit { get; set; }
        string Geo { get; set; }
        static int MsgId { get; set; }
        public double[] Value;
        DateTime Utc { get; set; }
        public string Topic { get; set; }

        public string MeasureName {
            get { return Type; }
        }

        public SensorMakerDen(string deviceName, string type, string unit, ValuesPerSample valuesPerSensor, string topic) {
            DeviceName = deviceName;
            Type = type;
            Unit = unit;
            Topic = topic;
            Value = new double[(int)valuesPerSensor];
        }

        public double[] Values() {
            return Value;
        }

        public void Location(string geo) {
            Geo = geo;
        }

        public byte[] ToJson() {
            SerialiseData();
            return jw.ToArray();
        }

        public override string ToString() {
            SerialiseData();
            return base.ToString();
        }

        private void SerialiseData() {
            DateTime now = DateTime.UtcNow;
            if (Util.utcOffset != null) { now = DateTime.UtcNow - Util.utcOffset; }

            jw.Begin();
            jw.AddProperty("Dev", DeviceName);
            jw.AddProperty("Type", Type);
            jw.AddProperty("Val", Value, 2);
            jw.AddProperty("Unit", Unit);
            jw.AddProperty("Utc", now);
            if (Geo != string.Empty) { jw.AddProperty("Geo", Geo); }
            jw.AddProperty("Id", MsgId++);
            jw.End();
        }


    }
}
