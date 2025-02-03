using IoT.Common;
using IoT.Interfaces;

namespace IoT.Domain.Sensor.Commands
{
    public record StoreSensorCommand() : ICommand
    {
        public required string SensorId { get; init; }

        public required double Value { get; init; }

        public required SensorValueUnit Unit { get; init; }

        public required DateTimeOffset TimestampRead { get; init; }

        public DateTimeOffset Timestamp { get; init; }
    }
}
