using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace IoT.Domain.Sensor.Events
{
    public class SensorEvent
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; init; }

        public required string AggregateId { get; init; }

        public string? SensorDbDataId { get; set; }

        public int Version { get; set; }

        public DateTimeOffset Timestamp { get; } = DateTimeOffset.Now;
    }
}
