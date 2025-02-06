using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace IoT.Persistence
{
    public class Snapshot
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = "";

        public required string AggregateId { get; init; }

        public required int Version { get; init; }

        public DateTimeOffset Timestamp { get; init; }

        public required byte[] Data { get; init; }
    }
}
