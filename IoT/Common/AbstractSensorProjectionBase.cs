using IoT.Domain.Sensor.Projections;
using IoT.Interfaces;
using IoT.Persistence.Events;
using MessagePack;

namespace IoT.Common
{
    [MessagePackObject]
    [Union(0, typeof(SensorProjectionBase))]
    [method: SerializationConstructor]
    public abstract class AbstractSensorProjectionBase(string id, UnitType unitType) : IProjection
    {
        [Key(0)]
        public string Id { get; } = id;

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
