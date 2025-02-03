using IoT.Interfaces;

namespace IoT.Domain.Sensor.Queries
{
    public record SensorQuery : IQuery
    {
        public required string SensorId { get; init; }
    }
}
