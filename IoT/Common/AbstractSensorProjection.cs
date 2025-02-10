using IoT.Domain.Sensor.Projections;
using IoT.Interfaces;
using IoT.Persistence.Events;
using MessagePack;

namespace IoT.Common
{
    [MessagePackObject]
    [Union(0, typeof(SensorDayProjection))]
    [Union(1, typeof(SensorHourProjection))]
    [method: SerializationConstructor]
    public abstract class AbstractSensorProjection(double value, DateTimeOffset timestamp) : IAggregate
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
