using IoT.Common;

namespace IoT.Domain.Sensor.Aggregates
{
    public class SensorHourAggregate : AbstractAggregate
    {
        public override Period Period { get; } = Period.Hourly;
    }
}
