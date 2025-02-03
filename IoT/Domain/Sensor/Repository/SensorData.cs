using IoT.Common;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace IoT.Domain.Sensor.Repository
{
    public class SensorData
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; init; }

        public required string SensorId { get; init; }

        public required double Value { get; init; }

        public required SensorValueUnit Unit { get; init; }

        public required DateTimeOffset TimestampRead { get; init; }

        public DateTimeOffset Timestamp { get; init; }
    }
}
