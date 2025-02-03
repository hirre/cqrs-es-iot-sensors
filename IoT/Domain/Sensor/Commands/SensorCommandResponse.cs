namespace IoT.Domain.Sensor.Commands
{
    public record SensorCommandResponse
    {
        public required IEnumerable<SensorDataCommandResponse> Data { get; init; }
    }

    public record SensorDataCommandResponse
    {
        public string? Id { get; init; }

        public required string SensorId { get; init; }

        public DateTimeOffset ResponseTimestamp { get; init; }
    }

}
