using IoT.Common;

namespace IoT.Domain.Sensor.Aggregates
{
    public class SensorDayAggregate
    {
        public required string SensorId { get; set; }

        public double Value { get; set; }

        public SensorValueUnit Unit { get; set; }

        public Period Period { get; set; }

        public DateTimeOffset Date { get; set; }
    }
}
