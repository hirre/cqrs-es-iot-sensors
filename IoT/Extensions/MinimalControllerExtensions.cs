using IoT.Domain.Sensor.Commands;
using IoT.Domain.Sensor.Queries;
using IoT.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace IoT.Extensions
{
    public static class MinimalControllerExtensions
    {
        public static void AddMinimalApiControllers(this WebApplication app)
        {
            app.MapGet("/api/Sensors", async ([FromServices] IQueryHandler<SensorQuery> handler, [AsParameters] SensorQuery query) =>
            {
                var res = await handler.HandleAsync(query);

                if (!res.IsSucceded)
                {
                    return Results.BadRequest(res.ErrorMessage);
                }

                return Results.Ok(res.Data);
            });

            app.MapPost("/api/Sensors", async ([FromServices] ICommandHandler<StoreSensorCommand> handler, [FromBody] StoreSensorCommand cmd) =>
            {
                var res = await handler.HandleAsync(cmd);

                if (!res.IsSucceded)
                {
                    return Results.BadRequest(res.ErrorMessage);
                }

                return Results.Ok(res.Data);
            });
        }
    }
}
