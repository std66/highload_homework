using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using TomiSoft.HighLoad.App.DataPersistence;
using TomiSoft.HighLoad.App.Models.Api;

namespace TomiSoft.HighLoad.App;

public static class ActionMethods {
    public static string Healthz() => "healthy";

    public static async Task<IResult> RegisterVehicle(RegisterVehicleRequestDto request, [FromServices] VehicleDataManager dataManager) {
        Guid id = await dataManager.RegisterVehicleAsync(request);
        return Results.Created($"/jarmuvek/{id}", null);
    }

    public static async Task<string> GetCountOfRegisteredVehicles([FromServices] VehicleDataManager dataManager) => (await dataManager.GetCountOfVehiclesAsync()).ToString();

    public static async Task<IResult> GetVehicleById([Required] Guid uuid, [FromServices] VehicleDataManager dataManager) {
        RegisteredVehicleDto? result = await dataManager.GetVehicleById(uuid);
        if (result is null)
            return Results.NotFound();

        return Results.Ok(result);
    }

    public static SearchVehicleResultDto SearchVehicle([FromQuery, Required] string q) {
        return new SearchVehicleResultDto() {
            GetTestData()
        };
    }

    private static RegisteredVehicleDto GetTestData() {
        return new RegisteredVehicleDto() {
            Uuid = Guid.NewGuid(),
            ForgalmiErvenyes = new DateOnly(2025, 02, 01),
            Rendszam = "ABC123",
            Tulajdonos = "Kiss István",
            Adatok = ["teszt1", "teszt2"]
        };
    }

    public static void AddVehicleApiServer(this WebApplication app) {
        app.MapGet("/healthz", ActionMethods.Healthz);

        app.MapPost("/jarmuvek", ActionMethods.RegisterVehicle);
        app.MapGet("/jarmuvek", ActionMethods.GetCountOfRegisteredVehicles);
        app.MapGet("/jarmuvek/{uuid}", ActionMethods.GetVehicleById);
        app.MapGet("/kereses", ActionMethods.SearchVehicle);
    }
}
