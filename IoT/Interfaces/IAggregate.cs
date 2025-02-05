using IoT.Persistence.Events;

namespace IoT.Interfaces
{
    public interface IAggregate
    {
        public void ApplyEvent(DomainEvent e, object? data = null);
    }
}
