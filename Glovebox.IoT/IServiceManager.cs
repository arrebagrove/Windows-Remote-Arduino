namespace Glovebox.IoT {
    public interface IServiceManager {
        uint Publish(string topic, byte[] data);
    }
}