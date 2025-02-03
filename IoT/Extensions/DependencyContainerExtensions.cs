using IoT.Domain.Sensor.Commands;
using IoT.Domain.Sensor.Handlers;
using IoT.Domain.Sensor.Queries;
using IoT.Domain.Sensor.Repository;
using IoT.Interfaces;
using IoT.Persistence;

namespace IoT.Extensions
{
    public static class DependencyContainerExtensions
    {
        public static void AddIotDependencies(this IServiceCollection services)
        {
            #region Sensors

            services.AddScoped<ICommandHandler<StoreSensorCommand, SensorCommandResponse>, SensorSensorCommandHandler>();
            services.AddScoped<IQueryHandler<SensorQuery, SensorQueryResponse>, SensorQueryHandler>();
            services.AddScoped<ISensorRepository, SensorRepository>();

            #endregion

            services.AddSingleton<MongoDbContext>();
        }
    }
}
