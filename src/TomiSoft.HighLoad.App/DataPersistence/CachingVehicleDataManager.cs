using Npgsql;
using System.Collections.Concurrent;
using TomiSoft.HighLoad.App.Models.Api;

namespace TomiSoft.HighLoad.App.DataPersistence;

public class CachingVehicleDataManager : VehicleDataManager {
    private readonly InMemoryCache memoryCache;

    public CachingVehicleDataManager(NpgsqlDataSource dataSource, InMemoryCache memoryCache) : base(dataSource) {
        this.memoryCache = memoryCache;
    }

    public override async Task<Guid> RegisterVehicleAsync(RegisterVehicleRequestDto registerRequest) {
        Guid id = await base.RegisterVehicleAsync(registerRequest);

        memoryCache.vehicleById.TryAdd(id, registerRequest);

        return id;
    }

    public override async Task<RegisteredVehicleDto?> GetVehicleById(Guid id) {
        bool cached = memoryCache.vehicleById.TryGetValue(id, out RegisterVehicleRequestDto? registerRequest);

        if (cached) {
            return new RegisteredVehicleDto() {
                Uuid = id,
                Adatok = registerRequest.Adatok,
                ForgalmiErvenyes = registerRequest.ForgalmiErvenyes,
                Rendszam = registerRequest.Rendszam,
                Tulajdonos = registerRequest.Tulajdonos,
            };
        }

        RegisteredVehicleDto? dto = await base.GetVehicleById(id);

        if (!cached && dto is not null) {
            memoryCache.vehicleById.TryAdd(id, new RegisterVehicleRequestDto() {
                Adatok = dto.Adatok,
                ForgalmiErvenyes = dto.ForgalmiErvenyes,
                Rendszam = dto.Rendszam,
                Tulajdonos = dto.Tulajdonos
            });
        }

        return dto;
    }
}

public class InMemoryCache {
    public readonly ConcurrentDictionary<Guid, RegisterVehicleRequestDto> vehicleById = new();
}
