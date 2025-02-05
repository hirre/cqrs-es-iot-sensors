using IoT.Common;
using MessagePack;

namespace IoT.Domain.Sensor.Aggregates
{
    [MessagePackObject]
    public class SensorHourAggregate(double value, DateTimeOffset timestamp) : AbstractSensorAggregate(value, timestamp)
    {
    }
}
