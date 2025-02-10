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

// Start a timer that wakes up at midnight every day and takes a snapshot of the current state of the projections
var timer = new Timer(async _ =>
{
    using var scope = app.Services.CreateScope();
    var sensorRepo = scope.ServiceProvider.GetRequiredService<ISensorRepository>();

    if (sensorRepo == null)
        return;

    var uniqueEntityIds = await sensorRepo.GetUniqueEntityIds();

    foreach (var tup in uniqueEntityIds)
    {
        await sensorRepo.TakeSnapShot(tup);
    }
}, null, TimeSpan.FromDays(1) - DateTime.Now.TimeOfDay, TimeSpan.FromDays(1));
//TODO: for testing purposes ---> }, null, TimeSpan.Zero, TimeSpan.FromSeconds(35));

app.Run();
