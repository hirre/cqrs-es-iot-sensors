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
                var sensorData = await _sensorRepository.StoreSensorDataAsync(command);

                var responseList = new List<SensorDataCommandResponse>();

                var now = DateTimeOffset.UtcNow;

                foreach (var sensorDataItem in sensorData)
                {
                    responseList.Add(new SensorDataCommandResponse
                    {
                        Id = sensorDataItem.Id,
                        SensorId = sensorDataItem.SensorId,
                        ResponseTimestamp = now,
                    });
                }

                return Result<SensorCommandResponse>.Success(new SensorCommandResponse() { Data = responseList });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error storing sensor data");
                return Result<SensorCommandResponse>.Failure(ex.Message);
            }
        }
    }
}
