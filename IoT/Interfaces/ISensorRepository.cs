using IoT.Domain.Sensor.Commands;
using IoT.Domain.Sensor.Events;

namespace IoT.Interfaces
{
    public interface ISensorRepository
    {
        public Task<(string? ObjId, SensorEvent Event)> StoreSensorDataAsync(StoreSensorDataCommand cmd);

        public Task HydrateReadModels(CancellationToken cancellationToken = default);

    }
}
