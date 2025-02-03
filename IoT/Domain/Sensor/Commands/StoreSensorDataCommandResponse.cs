namespace IoT.Domain.Sensor.Commands
{
    public record StoreSensorDataCommandResponse
    {
        public string? Id { get; init; }

        public required string SensorId { get; init; }

        public DateTimeOffset ResponseTimestamp { get; init; }
    }

}
