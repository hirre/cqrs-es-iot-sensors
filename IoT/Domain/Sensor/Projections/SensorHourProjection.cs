using IoT.Common;
using MessagePack;

namespace IoT.Domain.Sensor.Projections
{
    [MessagePackObject]
    public class SensorHourProjection(double value, DateTimeOffset timestamp) : AbstractSensorProjection(value, timestamp)
    {
    }
}
