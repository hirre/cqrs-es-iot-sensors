using IoT.Common;
using IoT.Domain.Sensor.Commands;
using IoT.Interfaces;

namespace IoT.Domain.Sensor.Handlers
{
    public class SensorStoreCommandHandler(ILogger<SensorStoreCommandHandler> logger,
        ISensorRepository sensorRepository)
        : ICommandHandler<StoreSensorDataCommand, StoreSensorDataCommandResponse>
    {
        private readonly ILogger<SensorStoreCommandHandler> _logger = logger;
        private readonly ISensorRepository _sensorRepository = sensorRepository;

        public async Task<Result<StoreSensorDataCommandResponse>> HandleAsync(StoreSensorDataCommand command)
        {
            try
            {
                var e = await _sensorRepository.StoreSensorDataAsync(command);

                return Result<StoreSensorDataCommandResponse>
                    .Success(new StoreSensorDataCommandResponse()
                    {
                        Id = e.Id,
                        SensorId = command.Id,
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
