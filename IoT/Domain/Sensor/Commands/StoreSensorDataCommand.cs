using IoT.Common;
using IoT.Interfaces;

namespace IoT.Domain.Sensor.Commands
{
    public record StoreSensorDataCommand() : ICommand
    {
        public required string Id { get; init; }

        public required UnitType SensorUnitType { get; init; }

        public required Period Period { get; init; }

        public DateTimeOffset Timestamp { get; init; }

        public required IEnumerable<SensorDataPoint> Data { get; init; }
    }

    public record SensorDataPoint()
    {
        public required double Value { get; init; }

        public required DateTimeOffset TimestampRead { get; init; }
    }
}
