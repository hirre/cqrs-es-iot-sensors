using IoT.Interfaces;
using IoT.Persistence.Events;

namespace IoT.Common
{
    public abstract class AbstractSensorAggregateRoot(string aggregateId, UnitType unitType) : IAggregate
    {
        public string AggregateId { get; } = aggregateId;

        public UnitType UnitType { get; } = unitType;

        public virtual void ApplyEvent(DomainEvent e, object? data = null)
        {
            // Default no behavior
        }
    }
}
