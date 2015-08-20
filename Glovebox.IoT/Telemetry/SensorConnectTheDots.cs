using Glovebox.IoT.Json;
using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;

namespace Glovebox.IoT.Telemetry {

    public sealed class SensorConnectTheDots : ISensorTelemetry {
        JSONWriter data = new JSONWriter();

        string guid { get; set; }
        string displayname { get; set; }
        string organization { get; set; }
        string location { get; set; }
        string measurename { get; set; }
        string unitofmeasure { get; set; }
        string timecreated { get; set; }
        double[] Value;
        public string Topic { get; set; }

        string ISensorTelemetry.MeasureName {
            get { return measurename; }
        }

        public SensorConnectTheDots() { }

        /// <summary>
        /// Connect the Dot Sensor Definition
        /// </summary>
        /// <param name="guid"></param>
        /// <param name="measurename"></param>
        /// <param name="unitofmeasure"></param>
        /// <param name="location"></param>
        /// <param name="organization"></param>
        /// <param name="displayname"></param>
        public SensorConnectTheDots(Guid guid, string measurename, string unitofmeasure, string organization, string displayname) {
            this.guid = guid.ToString();
            this.measurename = measurename;
            this.unitofmeasure = unitofmeasure;
            this.organization = organization;
            this.displayname = displayname;
            Value = new double[1];
        }

        public double[] Values() {
            return Value;
        }

        public void Location(string geo) {
            location = geo;
        }

        /// <summary>
        /// ToJson function is used to convert sensor data into a JSON string to be sent to Azure Event Hub
        /// </summary>
        /// <returns>JSon String containing all info for sensor data</returns>
        public byte[] ToJson() {
            SerialiseData();
            return data.ToArray();
        }

        public override string ToString() {
            SerialiseData();
            return data.ToString();
        }

        private void SerialiseData() {
            DateTime now = DateTime.UtcNow;
            if (Util.utcOffset != null) { now = DateTime.UtcNow - Util.utcOffset; }

            data.Begin();
            data.AddProperty("value", Value[0], 0);
            data.AddProperty("measurename", measurename);
            data.AddProperty("unitofmeasure", unitofmeasure);
            data.AddProperty("timecreated", now.ToString("o"));
            data.AddProperty("guid", guid);
            data.AddProperty("displayname", displayname);
            data.AddProperty("organization", organization);
            data.AddProperty("location", location);
            data.End();
        }
    }
}

