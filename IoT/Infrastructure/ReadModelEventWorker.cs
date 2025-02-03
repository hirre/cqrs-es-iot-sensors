using IoT.Domain.Sensor.Events;
using IoT.Persistence;
using Microsoft.Extensions.Caching.Distributed;

namespace IoT.Infrastructure
{
    public class ReadModelEventWorker(ILogger<ReadModelEventWorker> logger, SensorDbContext mongoDbContext,
        IDistributedCache distributedCache, ChannelQueue<SensorEvent> channelQueue) : BackgroundService
    {
        private readonly ILogger<ReadModelEventWorker> _logger = logger;
        private readonly IDistributedCache _distributedCache = distributedCache;
        private readonly SensorDbContext _mongoDbContext = mongoDbContext;
        private readonly ChannelQueue<SensorEvent> _channelQueue = channelQueue;

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("ReadModelEventWorker running.");

            while (!stoppingToken.IsCancellationRequested)
            {
                await foreach (var sensorEvent in _channelQueue.ReadAsync(stoppingToken))
                {
                    if (sensorEvent == null)
                    {
                        continue;
                    }

                    if (sensorEvent is SensorEvent sensorCmdEvent)
                    {
                        await ProcessSensorCmdEvent(sensorCmdEvent);
                    }
                    else
                    {
                        _logger.LogWarning("Unknown event type: {EventType}", sensorEvent.GetType().Name);
                    }
                }

            }

            _logger.LogInformation("ReadModelEventWorker stopping.");
        }

        private async Task ProcessSensorCmdEvent(SensorEvent sensorCmdEvent)
        {
            try
            {
                await Task.CompletedTask;
                _logger.LogDebug($"EVENT: {sensorCmdEvent.AggregateId}\t | " +
                    $"{sensorCmdEvent.SensorDbDataId}\t | " +
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
