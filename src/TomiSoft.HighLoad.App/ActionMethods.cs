using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using TomiSoft.HighLoad.App.Models.Api;

namespace TomiSoft.HighLoad.App;

public static class ActionMethods {
    public static string Healthz() => "healthy";

    public static IResult RegisterVehicle() {
        return Results.Created();
    }

    public static string GetCountOfRegisteredVehicles() => "0";

    public static RegisteredVehicleDto GetVehicleById(string uuid) {
        return GetTestData();
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

    public static void RegisterEndpoints(this WebApplication app) {
        app.MapGet("/healthz", ActionMethods.Healthz);

        app.MapPost("/jarmuvek", ActionMethods.RegisterVehicle);
        app.MapGet("/jarmuvek", ActionMethods.GetCountOfRegisteredVehicles);
        app.MapGet("/jarmuvek/{uuid}", ActionMethods.GetVehicleById);
        app.MapGet("/kereses", ActionMethods.SearchVehicle);
    }
}
