using IoT.Common;
using IoT.Domain.Sensor.Commands;
using IoT.Domain.Sensor.Events;
using IoT.Infrastructure;
using IoT.Interfaces;

namespace IoT.Domain.Sensor.Handlers
{
    public class SensorSensorCommandHandler(ILogger<SensorSensorCommandHandler> logger,
        ISensorRepository sensorRepository, ChannelQueue<SensorEvent> channelQueue)
        : ICommandHandler<StoreSensorDataCommand, StoreSensorDataCommandResponse>
    {
        private readonly ILogger<SensorSensorCommandHandler> _logger = logger;
        private readonly ISensorRepository _sensorRepository = sensorRepository;
        private readonly ChannelQueue<SensorEvent> _channelQueue = channelQueue;

        public async Task<Result<StoreSensorDataCommandResponse>> HandleAsync(StoreSensorDataCommand command)
        {
            try
            {
                var (ObjId, Event) = await _sensorRepository.StoreSensorDataAsync(command);

                // Publish events
                await _channelQueue
                    .PublishAsync(Event);

                return Result<StoreSensorDataCommandResponse>.Success(new StoreSensorDataCommandResponse()
                {
                    Id = ObjId,
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
