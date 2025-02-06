using IoT.Domain.Sensor.Aggregates;
using IoT.Interfaces;
using IoT.Persistence.Events;
using MessagePack;

namespace IoT.Common
{
    [MessagePackObject]
    [Union(0, typeof(SensorAggregateRoot))]
    [method: SerializationConstructor]
    public abstract class AbstractSensorAggregateRoot(string aggregateId, UnitType unitType) : IAggregate
    {
        [Key(0)]
        public string AggregateId { get; } = aggregateId;

        [Key(1)]
        public UnitType UnitType { get; } = unitType;

        [Key(2)]
        public int Version { get; set; }

        public virtual void ApplyEvent(DomainEvent e, object? data = null)
        {
            // Default no behavior
        }
    }
}
