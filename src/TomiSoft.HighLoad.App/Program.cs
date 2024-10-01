using Npgsql;
using System.Net;
using System.Text.Json;
using TomiSoft.HighLoad.App;
using TomiSoft.HighLoad.App.DataPersistence;
using TomiSoft.HighLoad.App.Models.Api;

ThreadPool.SetMinThreads(300, 300);

var builder = WebApplication.CreateSlimBuilder(args);

builder.Services
    .AddLogging(x => x.AddSimpleConsole(options => options.SingleLine = true))
    .AddSingleton<NpgsqlConnection>(sp => {
        var connectionString = builder.Configuration.GetConnectionString("postgres");
        return new NpgsqlConnection(connectionString);
    })
    .AddScoped<VehicleDataManager>()
    .AddTransient<IStartupFilter, PostgresInitializerStartupFilter>()
    .ConfigureHttpJsonOptions(options => {
        options.SerializerOptions.TypeInfoResolverChain.Insert(0, AppJsonSerializerContext.Default);
    });

var app = builder.Build();

app.Use(async (context, next) => {
    try {
        await next(context);
    }
    catch (NpgsqlException e) when (e.SqlState is "23505") { //unique constraint violation error by duplicate key
        context.Response.StatusCode = (int)HttpStatusCode.Conflict;
    }
    catch (Exception e) {
        context.Response.Headers.ContentType = "application/json";

        context.Response.StatusCode = e switch {
            BadHttpRequestException => (int)HttpStatusCode.BadRequest,
            _ => (int)HttpStatusCode.InternalServerError
        };

        await context.Response.WriteAsync(
            JsonSerializer.Serialize(new ErrorResponse(e.Message), AppJsonSerializerContext.Default.ErrorResponse)
        );
    }
});

app.AddVehicleApiServer();

app.Run();
