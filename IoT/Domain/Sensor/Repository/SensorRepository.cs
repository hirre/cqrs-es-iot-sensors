using IoT.Domain.Sensor.Commands;
using IoT.Domain.Sensor.Events;
using IoT.Infrastructure;
using IoT.Interfaces;
using IoT.Persistence;

namespace IoT.Domain.Sensor.Repository
{
    public class SensorRepository(ILogger<SensorRepository> logger, SensorDbContext mongoDbContext,
        ChannelQueue<SensorEvent> channelQueue) : ISensorRepository
    {
        private readonly ILogger<SensorRepository> _logger = logger;
        private readonly SensorDbContext _mongoDbContext = mongoDbContext;
        private readonly ChannelQueue<SensorEvent> _channelQueue = channelQueue;

        public async Task<(string? ObjId, SensorEvent Event)> StoreSensorDataAsync(StoreSensorDataCommand cmd)
        {
            var sensorDbData = new SensorDbData()
            {
                SensorId = cmd.SensorId,
                SenorUnitType = cmd.SensorUnitType,
                Period = cmd.Period,
                DataPoints = cmd.Data.Select(x => new SensorDbDataPoint()
                {
                    Value = x.Value,
                    TimestampRead = x.TimestampRead
                }).ToList()
            };

            var sEvent = new SensorEvent()
            {
                AggregateId =
                sensorDbData.SensorId
            };

            await _mongoDbContext.InsertSensorDataDocumentAsync(sensorDbData, sEvent);

            return (sensorDbData.Id, sEvent);
        }

        public async Task HydrateReadModels(CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Hydrating read models...");

                var eventBatch = await _mongoDbContext.GetSensorEventsAsync();

                while (await eventBatch.MoveNextAsync(cancellationToken))
                {
                    var current = eventBatch.Current;

                    foreach (var sensorEvent in current)
                    {
                        await _channelQueue.PublishAsync(sensorEvent);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while hydrating read models.");
                throw;
            }
        }
    }
}