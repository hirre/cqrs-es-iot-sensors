﻿using IoT.Domain.Sensor.Commands;
using IoT.Domain.Sensor.Events;
using IoT.Domain.Sensor.Handlers;
using IoT.Domain.Sensor.Queries;
using IoT.Domain.Sensor.Repository;
using IoT.Infrastructure;
using IoT.Interfaces;
using IoT.Persistence;

namespace IoT.Extensions
{
    public static class DependencyContainerExtensions
    {
        public static void AddIotDependencies(this IServiceCollection services)
        {
            #region Sensor DI

            services.AddScoped<ICommandHandler<StoreSensorDataCommand, StoreSensorDataCommandResponse>, SensorSensorCommandHandler>();
            services.AddScoped<IQueryHandler<SensorQuery, SensorQueryResponse>, SensorQueryHandler>();
            services.AddScoped<ISensorRepository, SensorRepository>();

            #endregion

            // Channel queue
            services.AddSingleton<ChannelQueue<SensorEvent>>();

            // Mongo DB
            services.AddSingleton<SensorDbContext>();

            // Redis Cache
            services.AddStackExchangeRedisCache((options) =>
            {
                options.Configuration = "localhost";
                options.ConfigurationOptions = new StackExchange.Redis.ConfigurationOptions()
                {
                    AbortOnConnectFail = true,
                    EndPoints = { options.Configuration }
                };
            });

            // Background service for event processing (normally you would use a message broker, like RabbitMQ)
            services.AddHostedService<ReadModelEventWorker>();
        }
    }
}
