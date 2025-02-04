using IoT.Common;

namespace IoT.Domain.Sensor.Aggregates
{
    public class SensorDayAggregate : AbstractAggregate
    {
        public override Period Period { get; } = Period.Daily;
    }
}
