using IoT.Common;
using IoT.Domain.Sensor.Projections;
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
                        var cacheKey = $"{e.EventType.ToEventPrefix()}:{e.EntityId}";

                        IProjection? projection = await CreateProjection(e, cacheKey);

                        if (projection != null)
                        {
                            projection.ApplyEvent(e);

                            await UpdateCache(projection, cacheKey);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error processing sensor event");
                    }

                    _logger.LogDebug($"EVENT: {e.Id}\t | " +
                        $"{e.EntityId}\t | " +
                        $"{e.Version}\t| " +
                        $"{e.Timestamp}");
                }
            }

            _logger.LogInformation("ReadModelEventWorker stopping.");
        }

        private async Task<IProjection?> CreateProjection(DomainEvent e, string cacheKey)
        {
            if (e.EventType == EventTypes.SensorStoreCmdEvent)
            {
                if (e.Payload is not SensorPayload payload)
                {
                    return null;
                }

                // Lookup projection in cache (or create if it doesn't exist)
                return await _distributedCache.GetOrSetDataAsync(cacheKey, () =>
                {
                    return Task.FromResult(new SensorProjectionBase(e.EntityId, payload.UnitType));
                });
            }

            return null;
        }

        private async Task UpdateCache(IProjection projection, string cacheKey)
        {
            if (projection is SensorProjectionBase sar)
            {
                // Update projection in cache
                await _distributedCache.SetDataAsync(cacheKey, sar);
            }
        }
    }
}
