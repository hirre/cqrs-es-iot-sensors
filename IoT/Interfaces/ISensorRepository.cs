using IoT.Domain.Sensor.Commands;
using IoT.Persistence.Events;

namespace IoT.Interfaces
{
    public interface ISensorRepository
    {
        public Task<Event> StoreSensorDataAsync(StoreSensorDataCommand cmd);

        public Task HydrateReadModels(CancellationToken cancellationToken = default);
    }
}
