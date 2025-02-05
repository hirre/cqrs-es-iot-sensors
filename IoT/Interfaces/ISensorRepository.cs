﻿using IoT.Common;
using IoT.Domain.Sensor.Commands;
using IoT.Persistence.Events;

namespace IoT.Interfaces
{
    public interface ISensorRepository
    {
        public Task<DomainEvent> StoreSensorDataAsync(StoreSensorDataCommand cmd);

        public Task<(UnitType UnitType, double Value)> GetLatestMonthlyAverageAsync(string aggregateId);

        public Task<(UnitType UnitType, double Value)> GetLatestDailyAverageAsync(string aggregateId);

        public Task HydrateReadModels(CancellationToken cancellationToken = default);
    }
}
