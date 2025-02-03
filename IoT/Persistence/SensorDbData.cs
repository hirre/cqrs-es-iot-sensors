using IoT.Common;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace IoT.Persistence
{
    public class SensorDbData
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; init; }

        public required string SensorId { get; init; }

        public required SensorValueUnit SenorUnitType { get; init; }

        public required Period Period { get; init; }

        public DateTimeOffset Timestamp { get; init; } = DateTimeOffset.UtcNow;

        public required IEnumerable<SensorDbDataPoint> DataPoints { get; init; }
    }

    public class SensorDbDataPoint
    {
        public required double Value { get; init; }
        public required DateTimeOffset TimestampRead { get; init; }
    }
}
