using IoT.Common;
using IoT.Domain.Sensor.Projections;
using IoT.Persistence.Events;
using MessagePack;
using System.Collections.Concurrent;

namespace IoT.Domain.Sensor.Aggregates
{
    [MessagePackObject]
    [method: SerializationConstructor]
    public partial class SensorProjectionBase(string aggregateId, UnitType unitType) : AbstractSensorProjectionBase(aggregateId, unitType)
    {
        private DateOnly _maxDate31Day = DateOnly.MinValue;
        private DateTime _maxDate24Hour = DateTime.MinValue;

        [Key(3)]
        public ConcurrentDictionary<DateOnly, SensorDayProjection> CyclicSensor31DayAggregates { get; } = [];

        [Key(4)]
        public ConcurrentDictionary<DateTime, SensorHourProjection> CyclicSensor24HourAggregates { get; } = [];

        [Key(5)]
        public double CalculatedMonthlyAverage { get; private set; }

        [Key(6)]
        public double CalculatedDailyAverage { get; private set; }

        public override void ApplyEvent(DomainEvent e, object? data = null)
        {
            if (e.EventType == EventTypes.SensorStoreCmdEvent && e.Payload is SensorPayload sensorPayload)
            {
                ApplySensorStoreCmdEvent(e, sensorPayload);
            }

            // Update version
            Version = e.Version;
        }

        private void ApplySensorStoreCmdEvent(DomainEvent e, SensorPayload sensorPayload)
        {
            if (sensorPayload == null)
            {
                return;
            }

            switch (sensorPayload.Period)
            {
                case Period.Hourly:

                    // When period is hourly, we get up to 24 hours of data
                    var hourlyDataPoints = sensorPayload.DataPoints.ToList();

                    if (hourlyDataPoints.Count == 0)
                    {
                        return;
                    }

                    foreach (var hourDataPoint in hourlyDataPoints)
                    {
                        if (hourDataPoint.TimestampRead.DateTime > _maxDate24Hour)
                        {
                            _maxDate24Hour = hourDataPoint.TimestampRead.DateTime;
                        }

                        var hourAggregate = new SensorHourProjection(hourDataPoint.Value, hourDataPoint.TimestampRead);
                        CyclicSensor24HourAggregates.TryAdd(hourDataPoint.TimestampRead.DateTime, hourAggregate);
                        hourAggregate.ApplyEvent(e);
                    }

                    CalculateDailyAverage();

                    break;

                case Period.Daily:

                    // When period is daily, we get one day of data
                    var dayDataPoint = sensorPayload.DataPoints.FirstOrDefault();

                    if (dayDataPoint == null)
                    {
                        return;
                    }

                    if (DateOnly.FromDateTime(dayDataPoint.TimestampRead.Date) > _maxDate31Day)
                    {
                        _maxDate31Day = DateOnly.FromDateTime(dayDataPoint.TimestampRead.Date);
                    }

                    var dayAggregate = new SensorDayProjection(dayDataPoint.Value, dayDataPoint.TimestampRead);
                    CyclicSensor31DayAggregates.TryAdd(DateOnly.FromDateTime(dayDataPoint.TimestampRead.DateTime), dayAggregate);
                    dayAggregate.ApplyEvent(e);

                    CalculateMonthlyAverage();

                    break;
            }
        }

        private void CalculateMonthlyAverage()
        {
            double sum = 0;
            var count = 0;

            var latestMonthBreakDate = _maxDate31Day.AddMonths(-1);

            foreach (var dayAggregateKey in CyclicSensor31DayAggregates.Keys.Where(x => x >= latestMonthBreakDate))
            {
                sum += CyclicSensor31DayAggregates[dayAggregateKey].Value;
                count++;
            }

            if (count == 0)
            {
                return;
            }

            CalculatedMonthlyAverage = (double)(sum / count);
        }

        private void CalculateDailyAverage()
        {
            double sum = 0;
            var count = 0;

            var latest24HourBreakDate = _maxDate24Hour.AddDays(-1);

            foreach (var hourAggregateKey in CyclicSensor24HourAggregates.Keys.Where(x => x >= latest24HourBreakDate))
            {
                sum += CyclicSensor24HourAggregates[hourAggregateKey].Value;
                count++;
            }

            if (count == 0)
            {
                return;
            }

            CalculatedDailyAverage = (double)(sum / count);
        }
    }
}
