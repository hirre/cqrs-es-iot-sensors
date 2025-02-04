using IoT.Domain.Sensor.Commands;
using IoT.Domain.Sensor.Handlers;
using IoT.Domain.Sensor.Queries;
using IoT.Domain.Sensor.Repository;
using IoT.Infrastructure;
using IoT.Interfaces;
using IoT.Persistence;
using IoT.Persistence.Events;

namespace IoT.Extensions
{
    public static class DependencyContainerExtensions
    {
        public static void AddIotDependencies(this IServiceCollection services, IConfiguration configuration)
        {
            #region Sensor DI

            services.AddScoped<ICommandHandler<StoreSensorDataCommand, StoreSensorDataCommandResponse>, SensorSensorCommandHandler>();
            services.AddScoped<IQueryHandler<SensorQuery, SensorQueryResponse>, SensorQueryHandler>();
            services.AddScoped<ISensorRepository, SensorRepository>();

            #endregion

            // Channel queue
            services.AddSingleton<ChannelQueue<DomainEvent>>();

            // Mongo DB
            services.AddSingleton<EventStore>();

            var redisEndpoint = configuration.GetSection("Redis:ConnectionString").Value;

            if (string.IsNullOrEmpty(redisEndpoint))
            {
                redisEndpoint = "localhost:6379";
            }

            // Redis Cache
            services.AddStackExchangeRedisCache((options) =>
            {
                options.ConfigurationOptions = new StackExchange.Redis.ConfigurationOptions()
                {
                    EndPoints = { redisEndpoint }
                };
            });

            // Background service for event processing (normally you would use a message broker, like RabbitMQ)
            services.AddHostedService<ReadModelEventWorker>();
        }
    }
}
