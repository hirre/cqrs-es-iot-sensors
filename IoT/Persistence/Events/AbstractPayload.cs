using MongoDB.Bson.Serialization.Attributes;

namespace IoT.Persistence.Events
{
    [BsonKnownTypes(typeof(SensorPayload))]
    public abstract class AbstractPayload
    {
    }
}
