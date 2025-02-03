using IoT.Domain.Sensor.Commands;
using IoT.Interfaces;
using IoT.Persistence;

namespace IoT.Domain.Sensor.Repository
{
    public class SensorRepository(ILogger<SensorRepository> logger, MongoDbContext mongoDbContext) : ISensorRepository
    {
        private readonly ILogger<SensorRepository> _logger = logger;
        private readonly MongoDbContext _mongoDbContext = mongoDbContext;

        public async Task<IEnumerable<SensorData>> StoreSensorDataAsync(StoreSensorCommand cmd)
        {
            var sensorDataList = new List<SensorData>();

            foreach (var sensorCmdData in cmd.Data)
            {
                sensorDataList.Add(new SensorData
                {
                    SensorId = sensorCmdData.SensorId,
                    Unit = sensorCmdData.Unit,
                    Value = sensorCmdData.Value,
                    Timestamp = sensorCmdData.Timestamp,
                    TimestampRead = sensorCmdData.TimestampRead,
                });
            }

            await _mongoDbContext.InsertSensorDataDocumentsAsync(sensorDataList);

            return sensorDataList;
        }
    }
}
