using Npgsql;
using System.Net;
using System.Text.Json;
using TomiSoft.HighLoad.App;
using TomiSoft.HighLoad.App.DataPersistence;
using TomiSoft.HighLoad.App.Models.Api;

ThreadPool.SetMinThreads(300, 300);

var builder = WebApplication.CreateSlimBuilder(args);

builder.Services
    .AddLogging(loggingBuilder => {
        loggingBuilder.AddJsonConsole();
    })
    .AddSingleton<NpgsqlDataSource>(sp => {
        var connectionString = builder.Configuration.GetConnectionString("postgres");
        return NpgsqlDataSource.Create(connectionString!);
    })
    .AddSingleton<InMemoryCache>()
    .AddScoped<VehicleDataManager>(
        x => new CachingVehicleDataManager(
            x.GetRequiredService<NpgsqlDataSource>(),
            x.GetRequiredService<InMemoryCache>()
        )
    )
    .ConfigureHttpJsonOptions(options => {
        options.SerializerOptions.TypeInfoResolverChain.Insert(0, AppJsonSerializerContext.Default);
    });

var app = builder.Build();

app.Use(async (context, next) => {
    var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
    using var _ = logger.BeginScope(new LogRequestIdState(context.Request.Headers["X-Request-Id"].ToString()));

    try {
        logger.LogInformation("Begin processing request");
        await next(context);
    }
    catch (Exception e) when (e.Message is "conflict") { //[CACHE] unique constraint violation error by duplicate key
        context.Response.StatusCode = (int)HttpStatusCode.Conflict;
    }
    catch (NpgsqlException e) when (e.SqlState is "23505") { //unique constraint violation error by duplicate key
        context.Response.StatusCode = (int)HttpStatusCode.Conflict;
    }
    catch (Exception e) {
        logger.LogError(e, "Unhandled exception: {0}", e.Message);

        context.Response.Headers.ContentType = "application/json";

        context.Response.StatusCode = e switch {
            BadHttpRequestException => (int)HttpStatusCode.BadRequest,
            _ => (int)HttpStatusCode.InternalServerError
        };

        await context.Response.WriteAsync(
            JsonSerializer.Serialize(new ErrorResponse(e.Message), AppJsonSerializerContext.Default.ErrorResponse)
        );
    }
    finally {
        logger.LogInformation("End processing request");
    }
});

app.AddVehicleApiServer();

app.Run();

file class LogRequestIdState : List<KeyValuePair<string, object>> {
    public LogRequestIdState(string requestId) {
        this.Add(new("X-Request-Id", requestId));
    }
    public override string ToString() => nameof(LogRequestIdState);
}