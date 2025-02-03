using IoT.Domain.Sensor.Events;
using IoT.Interfaces;
using IoT.Persistence;
using Microsoft.Extensions.Caching.Distributed;

namespace IoT.Infrastructure
{
    public class ReadModelEventWorker(ILogger<ReadModelEventWorker> logger, MongoDbContext mongoDbContext,
        IDistributedCache distributedCache, ChannelQueue<ISensorEvent> channelQueue) : BackgroundService
    {
        private readonly ILogger<ReadModelEventWorker> _logger = logger;
        private readonly IDistributedCache _distributedCache = distributedCache;
        private readonly MongoDbContext _mongoDbContext = mongoDbContext;
        private readonly ChannelQueue<ISensorEvent> _channelQueue = channelQueue;

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("ReadModelEventWorker running.");

            while (!stoppingToken.IsCancellationRequested)
            {
                var sensorEvent = await _channelQueue.WaitAndReadAsync(stoppingToken);

                if (sensorEvent == null)
                {
                    continue;
                }

                switch (sensorEvent)
                {
                    case SensorCmdEvent sensorCmdEvent:
                        await ProcessSensorCmdEvent(sensorCmdEvent);
                        break;
                    default:
                        _logger.LogWarning("Unknown event type: {EventType}", sensorEvent.GetType().Name);
                        break;
                }
            }

            _logger.LogInformation("ReadModelEventWorker stopping.");
        }

        private async Task ProcessSensorCmdEvent(SensorCmdEvent sensorCmdEvent)
        {
            try
            {
                var today = DateTimeOffset.UtcNow.Date;
                var sensorData = await _mongoDbContext.GetSensorDataDocumentsAsync(sensorCmdEvent.SensorId, today, today);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing sensor event");
            }
        }
    }
}
