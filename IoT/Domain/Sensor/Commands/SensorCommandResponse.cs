using IoT.Interfaces;

namespace IoT.Domain.Sensor.Commands
{
    public record SensorCommandResponse : IResponse
    {
        public required string SensorId { get; init; }

        public DateTimeOffset ResponseTimestamp { get; init; }
    }

}
