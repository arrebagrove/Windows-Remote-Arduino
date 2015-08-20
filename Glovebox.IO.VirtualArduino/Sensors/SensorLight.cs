using Glovebox.IoT.Base;
using Glovebox.IoT.Telemetry;
using Microsoft.Maker.RemoteWiring;

namespace Glovebox.IO.VirtualArduino.Sensors {
    public class VirtualLDR : SensorBase {

        RemoteDevice arduino;
        readonly UnoAnalogPins pin;

        // https://docs.google.com/spreadsheets/d/16MIFE4ITEisEDUBh3H4A7WZga1Sm1Pm-igS3r0A58L8/pub?gid=0#
        public enum UnoAnalogPins : byte {
            A0 = 14,
            A1 = 15,
            A2 = 16,
            A3 = 17,
            A4 = 18,
            A5 = 19
        };

        public VirtualLDR(RemoteDevice arduino, UnoAnalogPins pin, int SampleRateMilliseconds, string name) : base("light", "p", SensorMakerDen.ValuesPerSample.One, SampleRateMilliseconds, name) {
            this.arduino = arduino;
            this.pin = pin;

            StartMeasuring();
        }

        public override double Current {
            get {
                // pin mode must be reset everytime
                arduino.pinMode((byte)pin, PinMode.ANALOG);
                return arduino.analogRead((byte)(pin - UnoAnalogPins.A0));
            }
        }

        protected override string GeoLocation() {
            return string.Empty;
        }

        protected override void Measure(double[] value) {
            // pin mode must be reset everytime
            arduino.pinMode((byte)pin, PinMode.ANALOG);
            value[0] = arduino.analogRead((byte)(pin - UnoAnalogPins.A0));
        }

        protected override void SensorCleanup() {
        }
    }
}
