using IoT.Common;
using IoT.Domain.Sensor.Aggregates;
using IoT.Extensions;
using IoT.Persistence;
using IoT.Persistence.Events;
using Microsoft.Extensions.Caching.Distributed;

namespace IoT.Infrastructure
{
    public class ReadModelEventWorker(ILogger<ReadModelEventWorker> logger, EventStore eventStore,
        IDistributedCache distributedCache, ChannelQueue<DomainEvent> channelQueue) : BackgroundService
    {
        private readonly ILogger<ReadModelEventWorker> _logger = logger;
        private readonly IDistributedCache _distributedCache = distributedCache;
        private readonly EventStore _eventStore = eventStore;
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

                    switch (e.EventType)
                    {
                        case EventTypes.SensorStoreCmdEvent:
                            await ProcessSensorCmdEvent(e);
                            break;

                        default:
                            _logger.LogWarning("Unknown event type: {EventType}", e.GetType().Name);
                            break;
                    }

                    _logger.LogDebug($"EVENT: {e.Id}\t | " +
                        $"{e.AggregateId}\t | " +
                        $"{e.Version}\t| " +
                        $"{e.Timestamp}");
                }
            }

            _logger.LogInformation("ReadModelEventWorker stopping.");
        }

        private async Task ProcessSensorCmdEvent(DomainEvent sensorCmdEvent)
        {
            try
            {
                var sensorPayload = sensorCmdEvent.Payload as SensorPayload;

                if (sensorPayload == null)
                {
                    _logger.LogWarning("Sensor payload is null");
                    return;
                }

                var cacheKey = $"Sensor:{sensorCmdEvent.AggregateId}";

                // Lookup aggregate in cache (or create if it doesn't exist)
                var aggregate = await _distributedCache.GetOrSetDataAsync(cacheKey, () =>
                {
                    return Task.FromResult(new SensorAggregateRoot(sensorCmdEvent.AggregateId));
                });

                if (aggregate != null)
                {
                    aggregate.ApplyEvent(sensorCmdEvent);
                    await _distributedCache.SetDataAsync(cacheKey, aggregate);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing sensor event");
            }
        }
    }
}
