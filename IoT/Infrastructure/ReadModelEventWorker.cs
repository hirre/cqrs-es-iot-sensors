using IoT.Common;
using IoT.Domain.Sensor.Aggregates;
using IoT.Extensions;
using IoT.Interfaces;
using IoT.Persistence.Events;
using Microsoft.Extensions.Caching.Distributed;

namespace IoT.Infrastructure
{
    public class ReadModelEventWorker(ILogger<ReadModelEventWorker> logger,
            IDistributedCache distributedCache, ChannelQueue<DomainEvent> channelQueue) : BackgroundService
    {
        private readonly ILogger<ReadModelEventWorker> _logger = logger;
        private readonly IDistributedCache _distributedCache = distributedCache;
        private readonly ChannelQueue<DomainEvent> _channelQueue = channelQueue;

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("ReadModelEventWorker running.");

            while (!stoppingToken.IsCancellationRequested)
            {
                await foreach (var e in _channelQueue.ReadAsync(stoppingToken))
                {
                    if (e == null)
                    {
                        continue;
                    }

                    try
                    {
                        var cacheKey = $"AggregateId:{e.AggregateId}";

                        IAggregate? aggregate = await CreateAggregate(e, cacheKey);

                        if (aggregate != null)
                        {
                            aggregate.ApplyEvent(e);

                            await UpdateCache(aggregate, cacheKey);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error processing sensor event");
                    }

                    _logger.LogDebug($"EVENT: {e.Id}\t | " +
                        $"{e.AggregateId}\t | " +
                        $"{e.Version}\t| " +
                        $"{e.Timestamp}");
                }
            }

            _logger.LogInformation("ReadModelEventWorker stopping.");
        }

        private async Task<IAggregate?> CreateAggregate(DomainEvent e, string cacheKey)
        {
            if (e.EventType == EventTypes.SensorStoreCmdEvent)
            {
                if (e.Payload is not SensorPayload payload)
                {
                    return null;
                }

                // Lookup aggregate in cache (or create if it doesn't exist)
                return await _distributedCache.GetOrSetDataAsync(cacheKey, () =>
                {
                    return Task.FromResult(new SensorProjectionBase(e.AggregateId, payload.UnitType));
                });
            }

            return null;
        }

        private async Task UpdateCache(IAggregate aggregate, string cacheKey)
        {
            if (aggregate is SensorProjectionBase sar)
            {
                // Update aggregate in cache
                await _distributedCache.SetDataAsync(cacheKey, sar);
            }
        }
    }
}
