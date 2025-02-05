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
            app.MapGet("/api/Sensors", async ([FromServices] IQueryHandler<SensorGetLatestMonthlyAvgQuery, SensorQueryResponse> handler, [AsParameters] SensorGetLatestMonthlyAvgQuery query) =>
            {
                var res = await handler.HandleAsync(query);

                if (!res.IsSucceded)
                {
                    return Results.BadRequest(res.ErrorMessage);
                }

                return Results.Ok(res.Data);
            })
            .Produces<SensorQueryResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status500InternalServerError)
            .Produces(StatusCodes.Status400BadRequest);

            app.MapPost("/api/Sensors", async ([FromServices] ICommandHandler<StoreSensorDataCommand, StoreSensorDataCommandResponse> handler,
                [FromBody] StoreSensorDataCommand cmd) =>
            {
                var res = await handler.HandleAsync(cmd);

                if (!res.IsSucceded)
                {
                    return Results.BadRequest(res.ErrorMessage);
                }

                return Results.Ok(res.Data);
            })
            .Produces<StoreSensorDataCommandResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status500InternalServerError)
            .Produces(StatusCodes.Status400BadRequest);
        }
    }
}
