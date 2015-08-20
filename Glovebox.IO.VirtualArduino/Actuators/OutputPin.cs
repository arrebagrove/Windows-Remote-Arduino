using System;
using Glovebox.IoT.Base;
using Glovebox.IoT.Command;
using Microsoft.Maker.RemoteWiring;

namespace Glovebox.IO.VirtualArduino.Actuators {
    public class OutputPin : IotBase {

        RemoteDevice arduino;
        readonly byte pin;
        public enum Actions {
            On,
            Off
        }

        public OutputPin(RemoteDevice arduino, byte pin, string name) : base(name, "output", IotType.Actuator) {
            this.arduino = arduino;
            this.pin = pin;

            arduino.pinMode(pin, PinMode.OUTPUT);
        }

        protected override void CleanUp() {

        }

        public void Action(Actions action) {
            switch (action) {
                case Actions.On:
                    On();
                    break;
                case Actions.Off:
                    Off();
                    break;
                default:
                    break;
            }
        }

        public override void Action(IotAction action) {
            switch (action.cmd) {
                case "on":
                    On();
                    break;
                case "off":
                    Off();
                    break;
            }
        }

        public void On() {
            arduino.digitalWrite(pin, PinState.HIGH);
        }

        public void Off() {
            arduino.digitalWrite(pin, PinState.LOW);
        }
    }
}
