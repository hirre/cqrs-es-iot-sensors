using IoT.Common;
using IoT.Domain.Sensor.Queries;
using IoT.Interfaces;

namespace IoT.Domain.Sensor.Handlers
{
    public class SensorGetLatestDailyAvgQueryHandler(ILogger<SensorGetLatestDailyAvgQueryHandler> logger, ISensorRepository sensorRepository)
        : IQueryHandler<SensorGetLatestDailyAvgQuery, SensorQueryResponse>
    {
        private readonly ILogger<SensorGetLatestDailyAvgQueryHandler> _logger = logger;
        private readonly ISensorRepository _sensorRepository = sensorRepository;

        public async Task<Result<SensorQueryResponse>> HandleAsync(SensorGetLatestDailyAvgQuery query)
        {
            try
            {
                var result = await _sensorRepository.GetLatestDailyAverageAsync(query.AggregateId);

                return Result<SensorQueryResponse>.Success(new SensorQueryResponse()
                {
                    AggregateId = query.AggregateId,
                    UnitType = result.UnitType,
                    Value = result.Value
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting sensor data");
                return Result<SensorQueryResponse>.Failure(ex.Message);
            }
        }
    }
}
