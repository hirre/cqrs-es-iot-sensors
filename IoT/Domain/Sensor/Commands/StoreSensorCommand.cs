using IoT.Interfaces;

namespace IoT.Domain.Sensor.Commands
{
    public record StoreSensorCommand(string SensorId, double Value) : ICommand;

}
