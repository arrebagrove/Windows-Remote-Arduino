#if MF_FRAMEWORK_VERSION_V4_3
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using System.Threading;
#else
using System.Threading.Tasks;
#endif

using System;
using Glovebox.IoT.Json;
using Glovebox.IoT.Command;
using Glovebox.IoT.Telemetry;

namespace Glovebox.IoT.Base {
    public abstract class SensorBase : IotBase {
        public enum Actions { Start, Stop, Measure };
        ISensorTelemetry SensorData;

        protected abstract void Measure(double[] value);
        protected abstract string GeoLocation();
        public abstract double Current { get; }
        protected abstract void SensorCleanup();

        public delegate uint SensorEventHandler(object sender, EventArgs e);
        public event SensorEventHandler OnAfterMeasurement;
        public event SensorEventHandler OnBeforeMeasurement;


        public class SensorIdEventArgs : EventArgs {
            public readonly uint id;
            public SensorIdEventArgs(uint id) {
                this.id = id;
            }
        }

        public class SensorItemEventArgs : EventArgs {
            public ISensorTelemetry data;

            public SensorItemEventArgs(ISensorTelemetry data) {
                this.data = data;
            }
        }

#if MF_FRAMEWORK_VERSION_V4_3
        private Thread SensorThread;
#endif

        private static uint sensorErrorCount;
        public uint SensorErrorCount {
            get { return sensorErrorCount; }
        }

        private readonly string topicNamespace = ConfigurationManager.MqttNameSpace;
        private int sampleRateMilliseconds;

        public SensorBase(string sensorType, string sensorUnit, SensorMakerDen.ValuesPerSample valuesPerSensor, int SampleRateMilliseconds, string name)
            : base(name == null ? sensorType : name, sensorType, IotType.Sensor) {

            this.sampleRateMilliseconds = SampleRateMilliseconds;

            switch (ConfigurationManager.cloudMode) {
                case ConfigurationManager.Mode.MQTT_Maker:
                    SensorData = new SensorMakerDen(deviceName, sensorType, sensorUnit, valuesPerSensor, topicNamespace + deviceName + "/" + type);
                    break;
                case ConfigurationManager.Mode.EventHub_Enterprise:
                    SensorData = new SensorConnectTheDots(Guid.NewGuid(), sensorType, sensorUnit, ConfigurationManager.Organisation, sensorType);
                    break;
                default:
                    break;
            }


#if MF_FRAMEWORK_VERSION_V4_3
            SensorThread = new Thread(new ThreadStart(this.MeasureThread));
            SensorThread.Priority = ThreadPriority.Highest;
#endif
        }

        public void StartMeasuring() {
            if (sampleRateMilliseconds > 0) {
#if MF_FRAMEWORK_VERSION_V4_3
                SensorThread.Start();
#else
                Task.Run(() => MeasureThread());
#endif
            }
        }

        private void MeasureThread() {
            while (true) {
                try {
                    DoMeasure();
                }
                catch {
                    sensorErrorCount++;
                }
                Util.Delay(sampleRateMilliseconds);
            }
        }


        private void DoMeasure() {
            //lock (threadSync) {

            TotalSensorMeasurements++;
            BeforeMeasurement(new SensorIdEventArgs(id));
            Measure(SensorData.Values());
            SensorData.Location(ConfigurationManager.Location);
            sensorErrorCount = AfterMeasurement(new SensorItemEventArgs(SensorData));
            //}
        }


        private uint AfterMeasurement(EventArgs e) {
            if (OnAfterMeasurement != null) {
                return OnAfterMeasurement(this, e);
            }
            return 0;
        }

        private void BeforeMeasurement(EventArgs e) {
            if (OnBeforeMeasurement != null) { OnBeforeMeasurement(this, e); }
        }


        public override string ToString() {
            return SensorData.ToString();
        }

        public override void Action(IotAction action) {
            double sampleRate;
            if (action.cmd == null) { return; }
            switch (action.cmd) {
                case "measure":
                    DoMeasure();
                    break;
                case "start":
                    Action(Actions.Start);
                    break;
                case "stop":
                    Action(Actions.Stop);
                    break;
                case "rate":
                    //test for numeric sensor sample rate
                    if (action.parameters == null) { return; }
                    if (double.TryParse(action.parameters, out sampleRate)) {
                        Action((int)sampleRate);
                    }
                    break;
            }
        }

        public void Action(Actions action) {
#if MF_FRAMEWORK_VERSION_V4_3
            switch (action) {
                case Actions.Start:
                    if (SensorThread.ThreadState == ThreadState.Running) { return; }
                    if (sampleRateMilliseconds > 0) { SensorThread.Resume(); }
                    break;
                case Actions.Stop:
                    SensorThread.Suspend();
                    break;
                case Actions.Measure:
                    if (SensorThread.ThreadState == ThreadState.Running) { return; }
                    DoMeasure();
                    break;
                default:
                    break;
            }
#endif
        }

        public void Action(int sampleRateMilliseconds) {
            if (sampleRateMilliseconds > 0) {
                this.sampleRateMilliseconds = sampleRateMilliseconds;
            }
        }

        protected override void CleanUp() {
            SensorCleanup();
#if MF_FRAMEWORK_VERSION_V4_3
            SensorThread.Abort();
#endif
        }
    }
}
