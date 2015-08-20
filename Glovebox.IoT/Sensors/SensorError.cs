using Glovebox.IoT.Base;
using Glovebox.IoT.Telemetry;

namespace Glovebox.IoT.Sensors {
    public class SensorError : SensorBase {

        public override double Current { get { return (int)SensorErrorCount; } }

        public SensorError(int SampleRateMilliseconds, string name)
            : base("error", "n", SensorMakerDen.ValuesPerSample.One, SampleRateMilliseconds, name) {

            StartMeasuring();
        }

        protected override void Measure(double[] value) {
            value[0] = SensorErrorCount;
        }

        protected override string GeoLocation() {
            return string.Empty;
        }

        protected override void SensorCleanup() {
        }
    }
}
