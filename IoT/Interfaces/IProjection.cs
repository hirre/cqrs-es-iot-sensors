using IoT.Persistence.Events;

namespace IoT.Interfaces
{
    public interface IProjection
    {
        public void ApplyEvent(DomainEvent e, object? data = null);
    }
}
