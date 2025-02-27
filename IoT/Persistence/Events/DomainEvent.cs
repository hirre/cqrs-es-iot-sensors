﻿using IoT.Common;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace IoT.Persistence.Events
{
    public class DomainEvent
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = "";

        public required string EntityId { get; init; }

        public required EventTypes EventType { get; init; }

        public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.UtcNow;

        public int Version { get; set; }

        public AbstractPayload? Payload { get; set; }
    }
}
