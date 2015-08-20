namespace Glovebox.IoT.Telemetry {
    public interface ISensorTelemetry {

        string Topic { get; set; }

        void Location(string geo);
        byte[] ToJson();
        double[] Values();

        string MeasureName { get; }
    }
}