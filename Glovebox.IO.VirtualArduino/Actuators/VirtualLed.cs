using Microsoft.Maker.RemoteWiring;

namespace Glovebox.IO.VirtualArduino.Actuators {
    public class VirtualLed : OutputPin {

        public VirtualLed(RemoteDevice arduino, byte pin, string name) : base(arduino, pin, name) { }
    }
}
