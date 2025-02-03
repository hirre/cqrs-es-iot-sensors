using IoT.Common;
using IoT.Domain.Sensor.Queries;
using IoT.Interfaces;

namespace IoT.Domain.Sensor.Handlers
{
    public class SensorQueryHandler : IQueryHandler<SensorQuery, SensorQueryResponse>
    {
        public Task<Result<SensorQueryResponse>> HandleAsync(SensorQuery query)
        {
            throw new NotImplementedException();
        }
    }
}
