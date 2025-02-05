using IoT.Common;
using IoT.Domain.Sensor.Queries;
using IoT.Interfaces;

namespace IoT.Domain.Sensor.Handlers
{
    public class SensorGetLatestMonthlyAvgQueryHandler(ILogger<SensorGetLatestMonthlyAvgQueryHandler> logger, ISensorRepository sensorRepository)
        : IQueryHandler<SensorGetLatestMonthlyAvgQuery, SensorQueryResponse>
    {
        private readonly ILogger<SensorGetLatestMonthlyAvgQueryHandler> _logger = logger;
        private readonly ISensorRepository _sensorRepository = sensorRepository;

        public async Task<Result<SensorQueryResponse>> HandleAsync(SensorGetLatestMonthlyAvgQuery query)
        {
            try
            {
                var result = await _sensorRepository.GetLatestMonthlyAverageAsync(query.AggregateId);

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
