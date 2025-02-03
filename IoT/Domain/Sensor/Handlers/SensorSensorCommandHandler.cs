using IoT.Common;
using IoT.Domain.Sensor.Commands;
using IoT.Interfaces;

namespace IoT.Domain.Sensor.Handlers
{
    public class SensorSensorCommandHandler(ILogger<SensorSensorCommandHandler> logger, ISensorRepository sensorRepository)
        : ICommandHandler<StoreSensorCommand, SensorCommandResponse>
    {
        private readonly ILogger<SensorSensorCommandHandler> _logger = logger;
        private readonly ISensorRepository _sensorRepository = sensorRepository;

        public async Task<Result<SensorCommandResponse>> HandleAsync(StoreSensorCommand command)
        {
            try
            {
                var response = new SensorCommandResponse
                {
                    SensorId = command.SensorId,
                    ResponseTimestamp = DateTime.UtcNow,
                };

                // TODO: store the sensor data
                await Task.CompletedTask;

                return Result<SensorCommandResponse>.Success(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error storing sensor data");
                return Result<SensorCommandResponse>.Failure(ex.Message);
            }
        }
    }
}
