using IoT.Domain.Sensor.Commands;
using IoT.Domain.Sensor.Handlers;
using IoT.Domain.Sensor.Queries;
using IoT.Interfaces;

namespace IoT.Extensions
{
    public static class DependencyContainerExtensions
    {
        public static void AddDependencies(this IServiceCollection services)
        {
            services.AddScoped<ICommandHandler<StoreSensorCommand>, SensorSensorCommandHandler>();
            services.AddScoped<IQueryHandler<SensorQuery>, SensorQueryHandler>();
        }
    }
}
