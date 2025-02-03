using IoT.Common;
using IoT.Domain.Sensor.Commands;
using IoT.Interfaces;

namespace IoT.Domain.Sensor.Handlers
{
    public class SensorSensorCommandHandler : ICommandHandler<StoreSensorCommand>
    {
        public Task<Result<IResponse>> HandleAsync(StoreSensorCommand command)
        {
            throw new NotImplementedException();
        }
    }
}
