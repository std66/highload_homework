using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Text.Json;
using TomiSoft.HighLoad.App.Models.Api;

var builder = WebApplication.CreateSlimBuilder(args);

builder.Services.ConfigureHttpJsonOptions(options => {
    options.SerializerOptions.TypeInfoResolverChain.Insert(0, AppJsonSerializerContext.Default);
});

builder.Services
    .AddLogging();

var app = builder.Build();

app.Use(async (context, next) => {
    try {
        await next(context);
    }
    catch (Exception e) {
        context.Response.StatusCode = e switch {
            BadHttpRequestException => (int)HttpStatusCode.BadRequest,
            _ => (int)HttpStatusCode.InternalServerError
        };

        await context.Response.WriteAsync(
            JsonSerializer.Serialize(new ErrorResponse(e.Message), AppJsonSerializerContext.Default.ErrorResponse)
        );
    }
});

app.MapGet("/healthz", () => "healthy");

app.MapPost("/jarmuvek", () => {
    return Results.Created();
});

app.MapGet("/jarmuvek", () => "0");

app.MapGet("/jarmuvek/{uuid}", (string uuid) => {
    return GetTestData();
});

app.MapGet("/kereses", ([FromQuery, Required] string q) => {
    return new SearchVehicleResultDto() {
        GetTestData()
    };
});

app.Run();

RegisteredVehicleDto GetTestData() {
    return new RegisteredVehicleDto() {
        Uuid = Guid.NewGuid(),
        ForgalmiErvenyes = new DateTime(2025, 02, 01),
        Rendszam = "ABC123",
        Tulajdonos = "Kiss István",
        Adatok = ["teszt1", "teszt2"]
    };
}
