using IoT.Common;

namespace IoT.Domain.Sensor.Queries
{
    public record SensorQueryResponse
    {
        public required string Id { get; init; }

        public required UnitType UnitType { get; init; }

        public required double Value { get; init; }
    }
}
