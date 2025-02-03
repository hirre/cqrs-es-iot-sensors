using IoT.Interfaces;

namespace IoT.Domain.Sensor.Events
{
    public record SensorCmdEvent(string Id, string SensorId) : ISensorEvent
    {
        public string Id { get; } = Id;

        public string SensorId { get; } = SensorId;
    }
}
