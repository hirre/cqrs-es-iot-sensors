using IoT.Common;

namespace IoT.Persistence.Events
{
    public class SensorPayload : AbstractPayload
    {
        public required SensorValueUnit SenorUnitType { get; init; }

        public required Period Period { get; init; }

        public required IEnumerable<SensorDbDataPoint> DataPoints { get; init; }
    }

    public class SensorDbDataPoint
    {
        public required double Value { get; init; }
        public required DateTimeOffset TimestampRead { get; init; }
    }
}
