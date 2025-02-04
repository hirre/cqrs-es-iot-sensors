using IoT.Common;
using IoT.Domain.Sensor.Commands;
using IoT.Infrastructure;
using IoT.Interfaces;
using IoT.Persistence.Events;

namespace IoT.Domain.Sensor.Handlers
{
    public class SensorSensorCommandHandler(ILogger<SensorSensorCommandHandler> logger,
        ISensorRepository sensorRepository, ChannelQueue<Event> channelQueue)
        : ICommandHandler<StoreSensorDataCommand, StoreSensorDataCommandResponse>
    {
        private readonly ILogger<SensorSensorCommandHandler> _logger = logger;
        private readonly ISensorRepository _sensorRepository = sensorRepository;
        private readonly ChannelQueue<Event> _channelQueue = channelQueue;

        public async Task<Result<StoreSensorDataCommandResponse>> HandleAsync(StoreSensorDataCommand command)
        {
            try
            {
                var e = await _sensorRepository.StoreSensorDataAsync(command);

                // Publish events
                await _channelQueue
                    .PublishAsync(e);

                return Result<StoreSensorDataCommandResponse>
                    .Success(new StoreSensorDataCommandResponse()
                    {
                        Id = e.Id,
                        SensorId = command.SensorId,
                        ResponseTimestamp = DateTimeOffset.UtcNow
                    });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error storing sensor data");
                return Result<StoreSensorDataCommandResponse>.Failure(ex.Message);
            }
        }
    }
}
