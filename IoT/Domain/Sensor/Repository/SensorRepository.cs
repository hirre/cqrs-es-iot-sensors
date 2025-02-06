using IoT.Common;
using IoT.Domain.Sensor.Aggregates;
using IoT.Domain.Sensor.Commands;
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
                AggregateId = cmd.SensorId,
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

                var uniqueAggregates = await _eventStore.GetUniqueAggregateIdsAsync();

                foreach (var aggregateId in uniqueAggregates)
                {
                    // Restore the snapshots first
                    var snapshot = await _eventStore.GetLatestSnapshotAsync(aggregateId);

                    var startVersion = 0;

                    if (snapshot != null)
                    {
                        startVersion = snapshot.Version + 1;
                        var cacheKey = $"AggregateId:{snapshot.AggregateId}";
                        await _distributedCache.SetAsync(cacheKey, snapshot.Data);
                        _logger.LogInformation("Restored snapshot for aggregate {AggregateId}.", snapshot.AggregateId);
                    }

                    // Restore the rest of the events
                    var eventBatch = await _eventStore.GetEventsAsync(aggregateId, startVersion);

                    while (await eventBatch.MoveNextAsync(cancellationToken))
                    {
                        var domainEvent = eventBatch.Current;

                        foreach (var e in domainEvent)
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

        public Task<(UnitType, double)> GetLatestMonthlyAverageAsync(string aggregateId)
        {
            var cacheKey = $"AggregateId:{aggregateId}";

            if (_distributedCache.TryGetDataValue<SensorAggregateRoot>(cacheKey, out var sensorAggregateRoot))
            {
                if (sensorAggregateRoot == null)
                {
                    throw new Exception("Sensor aggregate root was null.");
                }

                return Task.FromResult((sensorAggregateRoot.UnitType, sensorAggregateRoot.CalculatedMonthlyAverage));
            }

            throw new Exception("Sensor aggregate root not found.");
        }

        public Task<(UnitType, double)> GetLatestDailyAverageAsync(string aggregateId)
        {
            var cacheKey = $"AggregateId:{aggregateId}";

            if (_distributedCache.TryGetDataValue<SensorAggregateRoot>(cacheKey, out var sensorAggregateRoot))
            {
                if (sensorAggregateRoot == null)
                {
                    throw new Exception("Sensor aggregate root was null.");
                }

                return Task.FromResult((sensorAggregateRoot.UnitType, sensorAggregateRoot.CalculatedDailyAverage));
            }

            throw new Exception("Sensor aggregate root not found.");
        }

        public async Task TakeSnapShot(string aggregateId)
        {
            var cacheKey = $"AggregateId:{aggregateId}";

            var aggregateRawData = await _distributedCache.GetAsync(cacheKey);

            if (aggregateRawData != null &&
                _distributedCache.TryGetDataValue<SensorAggregateRoot>(cacheKey, out var aggregateRoot) && aggregateRoot != null)
            {
                await _eventStore.StoreSnapShotAsync(aggregateRoot.AggregateId, aggregateRoot.Version, aggregateRawData);
            }
        }
    }
}