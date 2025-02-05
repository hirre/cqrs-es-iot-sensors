using IoT.Domain.Sensor.Aggregates;
using IoT.Interfaces;
using IoT.Persistence.Events;
using MessagePack;

namespace IoT.Common
{
    [MessagePackObject]
    [Union(0, typeof(SensorDayAggregate))]
    [Union(1, typeof(SensorHourAggregate))]
    [method: SerializationConstructor]
    public abstract class AbstractSensorAggregate(double value, DateTimeOffset timestamp) : IAggregate
    {
        [Key(0)]
        public double Value { get; } = value;

        [Key(1)]
        public DateTimeOffset Timestamp { get; } = timestamp;

        public virtual void ApplyEvent(DomainEvent e, object? data = null)
        {
            // Default no behavior
        }
    }
}
