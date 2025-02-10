using IoT.Common;
using MessagePack;

namespace IoT.Domain.Sensor.Projections
{
    [MessagePackObject]
    public class SensorDayProjection(double value, DateTimeOffset timestamp) : AbstractSensorProjection(value, timestamp)
    {
    }
}
