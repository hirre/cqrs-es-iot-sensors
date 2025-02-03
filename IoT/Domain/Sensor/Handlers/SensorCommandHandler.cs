using IoT.Common;
using IoT.Interfaces;

namespace IoT.Domain.Sensor.Handlers
{
    public class SensorCommandHandler : ICommandHandler
    {
        public Task<Result<IResponse>> HandleAsync(ICommand command)
        {
            throw new NotImplementedException();
        }
    }
}
