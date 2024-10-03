using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.Json;
using TomiSoft.HighLoad.App.DataPersistence;
using TomiSoft.HighLoad.App.Models.Api;

namespace TomiSoft.HighLoad.App;

public static class ActionMethods {
    public static string Healthz() => "healthy";

    public static async Task<IResult> RegisterVehicle(RegisterVehicleRequestDto request, [FromServices] VehicleDataManager dataManager, CancellationToken ct = default) {
        if (!request.IsValid)
            return Results.BadRequest();

        Guid id = await dataManager.RegisterVehicleAsync(request, ct);
        return Results.Created($"/jarmuvek/{id}", null);
    }

    public static async Task<string> GetCountOfRegisteredVehicles([FromServices] VehicleDataManager dataManager, CancellationToken ct = default) => (await dataManager.GetCountOfVehiclesAsync(ct)).ToString();

    public static async Task<IResult> GetVehicleById([Required] Guid uuid, [FromServices] VehicleDataManager dataManager, CancellationToken ct = default) {
        RegisteredVehicleDto? result = await dataManager.GetVehicleById(uuid, ct);
        if (result is null)
            return Results.NotFound();

        return Results.Ok(result);
    }

    public static IResult SearchVehicle([FromQuery, Required] string q, [FromServices] VehicleDataManager dataManager, CancellationToken ct = default) {
        if (string.IsNullOrEmpty(q))
            return Results.BadRequest();

        var vehicles = dataManager.SearchVehicle(q, ct);

        return Results.Stream(async (stream) => {
            await stream.WriteAsync(Encoding.UTF8.GetBytes("["), ct); // JSON tömb nyitó

            bool first = true;
            await foreach (var vehicle in vehicles.WithCancellation(ct)) {
                if (!first) {
                    await stream.WriteAsync(Encoding.UTF8.GetBytes(","), ct); // Vessző az elemek között
                }

                var json = JsonSerializer.Serialize(vehicle, AppJsonSerializerContext.Default.RegisteredVehicleDto);
                var buffer = Encoding.UTF8.GetBytes(json);
                await stream.WriteAsync(buffer, ct);

                first = false;
            }

            await stream.WriteAsync(Encoding.UTF8.GetBytes("]"), ct); // JSON tömb záró
        }, "application/json");
    }


    public static void AddVehicleApiServer(this WebApplication app) {
        app.MapGet("/healthz", ActionMethods.Healthz);

        app.MapPost("/jarmuvek", ActionMethods.RegisterVehicle);
        app.MapGet("/jarmuvek", ActionMethods.GetCountOfRegisteredVehicles);
        app.MapGet("/jarmuvek/{uuid}", ActionMethods.GetVehicleById);
        app.MapGet("/kereses", ActionMethods.SearchVehicle);
    }
}
