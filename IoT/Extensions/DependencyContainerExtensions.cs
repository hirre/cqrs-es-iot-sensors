using IoT.Domain.Sensor.Commands;
using IoT.Domain.Sensor.Handlers;
using IoT.Domain.Sensor.Queries;
using IoT.Domain.Sensor.Repository;
using IoT.Interfaces;

namespace IoT.Extensions
{
    public static class DependencyContainerExtensions
    {
        public static void AddDependencies(this IServiceCollection services)
        {
            #region Sensors

            services.AddScoped<ICommandHandler<StoreSensorCommand, SensorCommandResponse>, SensorSensorCommandHandler>();
            services.AddScoped<IQueryHandler<SensorQuery>, SensorQueryHandler>();
            services.AddScoped<ISensorRepository, SensorRepository>();

            #endregion
        }
    }
}
