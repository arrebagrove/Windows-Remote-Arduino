using Microsoft.Maker.RemoteWiring;

namespace Glovebox.IO.VirtualArduino.Actuators {
    public class VirtualRelay : OutputPin {

        public VirtualRelay(RemoteDevice arduino, byte pin, string name) : base(arduino, pin, name) { }
    }
}
