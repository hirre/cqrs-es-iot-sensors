using IoT.Common;
using IoT.Domain.Sensor.Commands;
using IoT.Domain.Sensor.Projections;
using IoT.Extensions;
using IoT.Infrastructure;
using IoT.Interfaces;
using IoT.Persistence;
using IoT.Persistence.Events;
using Microsoft.Extensions.Caching.Distributed;

namespace IoT.Domain.Sensor.Repository
{
    public class SensorRepository(ILogger<SensorRepository> logger, EventStore eventStore,
        ChannelQueue<DomainEvent> channelQueue,
        IDistributedCache distributedCache) : ISensorRepository
    {
        private readonly ILogger<SensorRepository> _logger = logger;
        private readonly EventStore _eventStore = eventStore;
        private readonly ChannelQueue<DomainEvent> _channelQueue = channelQueue;
        private readonly IDistributedCache _distributedCache = distributedCache;

        public async Task<DomainEvent> StoreSensorDataAsync(StoreSensorDataCommand cmd)
        {
            var e = new DomainEvent()
            {
                EntityId = cmd.Id,
                Timestamp = DateTimeOffset.UtcNow,
                EventType = EventTypes.SensorStoreCmdEvent,
                Payload = new SensorPayload()
                {
                    UnitType = cmd.SensorUnitType,
                    Period = cmd.Period,
                    DataPoints = cmd.Data.Select(x => new SensorDbDataPoint()
                    {
                        Value = x.Value,
                        TimestampRead = x.TimestampRead
                    }).ToList()
                }
            };

            await _eventStore.AppendEventAsync(e);

            await _channelQueue.PublishAsync(e);

            return e;
        }

        public async Task HydrateReadModels(CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Hydrating read models...");

                var uniqueEntityIds = await _eventStore.GetUniqueEntityIdsAsync();

                foreach (var tup in uniqueEntityIds)
                {
                    var evType = tup.EvType.ToEventPrefix();

                    // Restore the snapshots first
                    var snapshot = await _eventStore.GetLatestSnapshotAsync(evType, tup.Id);

                    var startVersion = 0;

                    if (snapshot != null)
                    {
                        startVersion = snapshot.Version + 1;
                        var cacheKey = $"{evType}:ID:{snapshot.EntityId}";
                        await _distributedCache.SetAsync(cacheKey, snapshot.Data);
                        _logger.LogInformation("Restored snapshot for entity {EntityId}.", snapshot.EntityId);
                    }

                    // Restore the rest of the events
                    var eventBatch = await _eventStore.GetEventsAsync(tup.EvType, tup.Id, startVersion);

                    while (await eventBatch.MoveNextAsync(cancellationToken))
                    {
                        var domainEvents = eventBatch.Current;

                        foreach (var e in domainEvents)
                        {
                            await _channelQueue.PublishAsync(e);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while hydrating read models.");
                throw;
            }
        }

        public Task<(UnitType, double)> GetLatestMonthlyAverageAsync(string id)
        {
            var cacheKey = $"Senor:ID:{id}";

            if (_distributedCache.TryGetDataValue<SensorProjectionBase>(cacheKey, out var sensorProjectionBase))
            {
                if (sensorProjectionBase == null)
                {
                    throw new Exception("Sensor projection base was null.");
                }

                return Task.FromResult((sensorProjectionBase.UnitType, sensorProjectionBase.CalculatedMonthlyAverage));
            }

            throw new Exception("Sensor projection base not found.");
        }

        public Task<(UnitType, double)> GetLatestDailyAverageAsync(string id)
        {
            var cacheKey = $"Sensor:ID:{id}";

            if (_distributedCache.TryGetDataValue<SensorProjectionBase>(cacheKey, out var sensorProjectionBase))
            {
                if (sensorProjectionBase == null)
                {
                    throw new Exception("Sensor projection base was null.");
                }

                return Task.FromResult((sensorProjectionBase.UnitType, sensorProjectionBase.CalculatedDailyAverage));
            }

            throw new Exception("Sensor projection base not found.");
        }

        public async Task TakeSnapShot((EventTypes EvType, string Id) tup)
        {
            var eventPrefix = tup.EvType.ToEventPrefix();
            var cacheKey = $"{eventPrefix}:ID:{tup.Id}";

            var projectionRawData = await _distributedCache.GetAsync(cacheKey);

            if (projectionRawData != null &&
                _distributedCache.TryGetDataValue<SensorProjectionBase>(cacheKey, out var projectionBase) && projectionBase != null)
            {
                _logger.LogInformation($"Taking snapshot for projection {eventPrefix}:{tup.Id}...");
                await _eventStore.StoreSnapShotAsync(eventPrefix, projectionBase.Id, projectionBase.Version, projectionRawData);
            }
        }

        public Task<IEnumerable<(EventTypes EvType, string Id)>> GetUniqueEntityIds()
        {
            return _eventStore.GetUniqueEntityIdsAsync();
        }
    }
}