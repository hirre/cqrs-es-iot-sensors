using IoT.Interfaces;
using IoT.Persistence.Events;

namespace IoT.Common
{
    public abstract class AbstractSensorAggregate(double value, DateTimeOffset timestamp) : IAggregate
    {
        public double Value { get; } = value;

        public DateTimeOffset Timestamp { get; } = timestamp;

        public virtual void ApplyEvent(DomainEvent e, object? data = null)
        {
            // Default no behavior
        }
    }
}
