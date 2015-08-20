using Glovebox.IoT.Base;
using Glovebox.IoT.Command;
using Glovebox.IoT.Telemetry;
#if MF_FRAMEWORK_VERSION_V4_3
using Microsoft.SPOT;
#endif

namespace Glovebox.IoT.Sensors {
    public class SensorDiagnostics : SensorBase {

        public SensorDiagnostics(int SampleRateMilliseconds, string name)
            : base("diag", "g", SensorMakerDen.ValuesPerSample.Five, SampleRateMilliseconds, name) {

            StartMeasuring();
        }

        protected override void Measure(double[] value) {
            value[0] = SensorErrorCount;
            value[1] = IotActionManager.ActionErrorCount;
            value[2] = TotalSensorMeasurements;
            value[3] = IotActionManager.TotalActions;
#if MF_FRAMEWORK_VERSION_V4_3
            value[4] = Debug.GC(false);
#else
            value[4] = 0d;
#endif
        }

        protected override string GeoLocation() {
            return string.Empty;
        }

        public override double Current {
#if MF_FRAMEWORK_VERSION_V4_3
            get { return Debug.GC(false); }
#else
            get { return 0d; }
#endif

        }

        protected override void SensorCleanup() {
        }
    }
}
