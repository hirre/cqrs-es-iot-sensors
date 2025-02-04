using IoT.Common;
using IoT.Domain.Sensor.Commands;
using IoT.Infrastructure;
using IoT.Interfaces;
using IoT.Persistence;
using IoT.Persistence.Events;

namespace IoT.Domain.Sensor.Repository
{
    public class SensorRepository(ILogger<SensorRepository> logger, EventStore eventStore,
        ChannelQueue<Event> channelQueue) : ISensorRepository
    {
        private readonly ILogger<SensorRepository> _logger = logger;
        private readonly EventStore _eventStore = eventStore;
        private readonly ChannelQueue<Event> _channelQueue = channelQueue;

        public async Task<Event> StoreSensorDataAsync(StoreSensorDataCommand cmd)
        {
            var e = new Event()
            {
                AggregateId = cmd.SensorId,
                Timestamp = DateTimeOffset.UtcNow,
                EventType = EventTypes.SensorStoreCmdEvent,
                Payload = new SensorPayload()
                {
                    SenorUnitType = cmd.SensorUnitType,
                    Period = cmd.Period,
                    DataPoints = cmd.Data.Select(x => new SensorDbDataPoint()
                    {
                        Value = x.Value,
                        TimestampRead = x.TimestampRead
                    }).ToList()
                }
            };

            await _eventStore.AppendEventAsync(e);

            return e;
        }

        public async Task HydrateReadModels(CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Hydrating read models...");

                var eventBatch = await _eventStore.GetEventsAsync();

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