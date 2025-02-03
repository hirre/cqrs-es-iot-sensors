using IoT.Domain.Sensor.Commands;
using IoT.Domain.Sensor.Repository;

namespace IoT.Interfaces
{
    public interface ISensorRepository
    {
        public Task<IEnumerable<SensorData>> StoreSensorDataAsync(StoreSensorCommand cmd);
    }
}
