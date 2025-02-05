using IoT.Common;

namespace IoT.Domain.Sensor.Aggregates
{
    public class SensorHourAggregate(double value, DateTimeOffset timestamp) : AbstractSensorAggregate(value, timestamp)
    {
    }
}
