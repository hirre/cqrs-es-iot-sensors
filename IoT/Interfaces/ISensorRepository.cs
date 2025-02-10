using IoT.Common;
using IoT.Domain.Sensor.Commands;
using IoT.Persistence.Events;

namespace IoT.Interfaces
{
    public interface ISensorRepository
    {
        public Task<DomainEvent> StoreSensorDataAsync(StoreSensorDataCommand cmd);

        public Task<(UnitType UnitType, double Value)> GetLatestMonthlyAverageAsync(string id);

        public Task<(UnitType UnitType, double Value)> GetLatestDailyAverageAsync(string id);

        public Task HydrateReadModels(CancellationToken cancellationToken = default);

        public Task TakeSnapShot((EventTypes EvType, string Id) tup);

        public Task<IEnumerable<(EventTypes EvType, string Id)>> GetUniqueEntityIds();
    }
}
