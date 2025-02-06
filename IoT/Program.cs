using IoT.Extensions;
using IoT.Interfaces;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddIotDependencies(builder.Configuration);

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.AddMinimalApiControllers();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var sensorRepo = scope.ServiceProvider.GetRequiredService<ISensorRepository>();

    if (sensorRepo != null)
        await sensorRepo.HydrateReadModels();
}

// Start a timer that wakes up at midnight every day and takes a snapshot of the current state of the aggregates
var timer = new Timer(async _ =>
{
    using var scope = app.Services.CreateScope();
    var sensorRepo = scope.ServiceProvider.GetRequiredService<ISensorRepository>();

    if (sensorRepo == null)
        return;

    var uniqueAggregates = await sensorRepo.GetUniqueAggregateIds();

    foreach (var aggregateId in uniqueAggregates)
    {
        await sensorRepo.TakeSnapShot(aggregateId);
    }
    //}, null, TimeSpan.Zero, TimeSpan.FromSeconds(10));  // For testing purposes, take a snapshot every 10 seconds
}, null, TimeSpan.Zero, TimeSpan.FromDays(1));

app.Run();
