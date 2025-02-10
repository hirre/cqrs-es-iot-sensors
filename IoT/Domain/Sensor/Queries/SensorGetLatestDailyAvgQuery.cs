using IoT.Interfaces;

namespace IoT.Domain.Sensor.Queries
{
    public record SensorGetLatestDailyAvgQuery : IQuery
    {
        public required string Id { get; init; }
    }
}
