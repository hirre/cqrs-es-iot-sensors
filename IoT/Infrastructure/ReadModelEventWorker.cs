using IoT.Common;
using IoT.Persistence;
using IoT.Persistence.Events;
using Microsoft.Extensions.Caching.Distributed;

namespace IoT.Infrastructure
{
    public class ReadModelEventWorker(ILogger<ReadModelEventWorker> logger, EventStore eventStore,
        IDistributedCache distributedCache, ChannelQueue<Event> channelQueue) : BackgroundService
    {
        private readonly ILogger<ReadModelEventWorker> _logger = logger;
        private readonly IDistributedCache _distributedCache = distributedCache;
        private readonly EventStore _eventStore = eventStore;
        private readonly ChannelQueue<Event> _channelQueue = channelQueue;

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
                }
            }

            _logger.LogInformation("ReadModelEventWorker stopping.");
        }

        private async Task ProcessSensorCmdEvent(Event sensorCmdEvent)
        {
            try
            {
                var sensorPayload = sensorCmdEvent.Payload as SensorPayload;

                // TODO: remove
                await Task.CompletedTask;

                _logger.LogDebug($"EVENT: {sensorCmdEvent.Id}\t | " +
                    $"{sensorCmdEvent.AggregateId}\t | " +
                    $"{sensorCmdEvent.Version}\t| " +
                    $"{sensorCmdEvent.Timestamp}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing sensor event");
            }
        }
    }
}
