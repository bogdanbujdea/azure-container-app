using Exercises.API;
using Exercises.Storage;
using Exercises.Storage.Entities;
using Gremlin.Net.Driver;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHealthChecks()
    .AddCheck<StartupHealthCheck>("Startup", tags: new[] { "startup" })
    .AddCheck<ReadyHealthCheck>("Ready", tags: new[] { "ready" });
builder.Configuration.AddEnvironmentVariables();

builder.Services.AddDbContext<UsersDbContext>(options =>
{
    var connectionString = builder.Configuration["SQL_CONNECTION_STRING"];
    options.UseSqlServer(connectionString);
});

var app = builder.Build();
GremlinWrapper.Initialize(builder.Configuration);

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weather", () =>
    {
        Console.WriteLine($"Retrieving weather");
        var forecast = Enumerable.Range(1, 5).Select(index =>
                new WeatherForecast
                (
                    DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                    Random.Shared.Next(-20, 55),
                    summaries[Random.Shared.Next(summaries.Length)]
                ))
            .ToArray();
        return forecast;
    })
    .WithName("GetWeatherForecast")
    .WithOpenApi();
var users = new List<ApplicationUser>();

app.MapGet("/", () =>
{
    Console.WriteLine($"Calling root endpoint");
    return "Hello Azure Container App!";
});
app.MapGet("/users", () =>
{
    Console.WriteLine($"Retrieving users");
    return users;
});
app.MapPost("/users", (ApplicationUser user) =>
{
    Console.WriteLine($"Adding user");
    user.Id = Guid.NewGuid();
    users.Add(user);

    return user;
});
app.MapPost("/exerciseRoutine", async (string exerciseRoutineName, string? gymName) =>
{
    try
    {
        var exerciseRoutineResult = await GremlinWrapper.CreateEntity("exerciseRoutine", exerciseRoutineName);
        if (gymName == null) return exerciseRoutineResult;
        var gymResult = await GremlinWrapper.CreateEntity("gym", gymName);
        await GremlinWrapper.LinkEntities(exerciseRoutineName, gymName, "availableAt");
        return gymResult;
    }
    catch (Exception ex)
    {
        Console.WriteLine(ex.ToString());
        throw;
    }

});

app.MapPost("/gyms", async (string gymName) =>
{
    try
    {
        var result = await GremlinWrapper.CreateEntity("gym", gymName);
        return result;
    }
    catch (Exception ex)
    {
        Console.WriteLine(ex.ToString());
        throw;
    }

});
app.MapPost("/exercise", async (string exerciseName, string? exerciseRoutineName, int? reps, string? gymName) =>
{
    Console.WriteLine($"Adding exercise");

    try
    {
        var result = await GremlinWrapper.CreateEntity("exercise", exerciseName);

        if (string.IsNullOrWhiteSpace(exerciseRoutineName) == false && reps != null)
        {
            if (await GremlinWrapper.VertexExistsAsync(exerciseRoutineName) == false)
            {
                await GremlinWrapper.CreateEntity("exerciseRoutine", exerciseRoutineName);
            }
            await GremlinWrapper.LinkEntities(exerciseRoutineName, exerciseName, "includes");
            if (gymName != null)
            {
                if (await GremlinWrapper.VertexExistsAsync(gymName) == false)
                {
                    await GremlinWrapper.CreateEntity("gym", gymName);
                }
                await GremlinWrapper.LinkEntities(exerciseRoutineName, gymName, "availableAt");
            }
        }

        return result;
    }
    catch (Exception ex)
    {
        Console.WriteLine(ex.ToString());
        throw;
    }

});


app.MapHealthChecks("/healthz/ready", new HealthCheckOptions
{
    Predicate = healthCheck => healthCheck.Tags.Contains("ready")
});

app.MapHealthChecks("/healthz/live", new HealthCheckOptions
{
    Predicate = _ => false
});
app.MapHealthChecks("/healthz/startup", new HealthCheckOptions
{
    Predicate = healthCheck => healthCheck.Tags.Contains("startup")
});
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<UsersDbContext>();
    await dbContext.Database.MigrateAsync();
}

app.Run();