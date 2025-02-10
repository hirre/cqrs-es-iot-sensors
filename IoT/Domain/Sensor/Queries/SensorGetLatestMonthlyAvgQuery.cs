using IoT.Interfaces;

namespace IoT.Domain.Sensor.Queries
{
    public record SensorGetLatestMonthlyAvgQuery : IQuery
    {
        public required string Id { get; init; }
    }
}
