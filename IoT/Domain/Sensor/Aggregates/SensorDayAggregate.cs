using IoT.Common;

namespace IoT.Domain.Sensor.Aggregates
{
    public class SensorDayAggregate(double value, DateTimeOffset timestamp) : AbstractSensorAggregate(value, timestamp)
    {
    }
}
