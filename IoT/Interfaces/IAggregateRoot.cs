using IoT.Persistence.Events;

namespace IoT.Interfaces
{
    public interface IAggregateRoot
    {
        public void ApplyEvent(DomainEvent e);
    }
}
