using Npgsql;
using TomiSoft.HighLoad.App.Models.Api;

namespace TomiSoft.HighLoad.App.DataPersistence;

public class CachingVehicleDataManager : VehicleDataManager {
    private readonly InMemoryCache memoryCache;

    public CachingVehicleDataManager(NpgsqlDataSource dataSource, InMemoryCache memoryCache) : base(dataSource) {
        this.memoryCache = memoryCache;
    }

    public override async Task<Guid> RegisterVehicleAsync(RegisterVehicleRequestDto registerRequest, CancellationToken ct = default) {
        if (memoryCache.rendszamok.Contains(registerRequest.Rendszam))
            throw new Exception("conflict");

        Guid id = await base.RegisterVehicleAsync(registerRequest, ct);

        memoryCache.vehicleById.TryAdd(id, registerRequest);
        memoryCache.rendszamok.Add(registerRequest.Rendszam);

        return id;
    }

    public override async Task<RegisteredVehicleDto?> GetVehicleById(Guid id, CancellationToken ct = default) {
        bool cached = memoryCache.vehicleById.TryGetValue(id, out RegisterVehicleRequestDto? registerRequest);

        if (cached && registerRequest is not null) {
            return new RegisteredVehicleDto() {
                Uuid = id,
                Adatok = registerRequest.Adatok,
                ForgalmiErvenyes = registerRequest.ForgalmiErvenyes,
                Rendszam = registerRequest.Rendszam,
                Tulajdonos = registerRequest.Tulajdonos,
            };
        }

        RegisteredVehicleDto? dto = await base.GetVehicleById(id, ct);

        if (!cached && dto is not null) {
            memoryCache.vehicleById.TryAdd(id, new RegisterVehicleRequestDto() {
                Adatok = dto.Adatok,
                ForgalmiErvenyes = dto.ForgalmiErvenyes,
                Rendszam = dto.Rendszam,
                Tulajdonos = dto.Tulajdonos
            });

            memoryCache.rendszamok.Add(dto.Rendszam);
        }

        return dto;
    }
}

public class InMemoryCache {
    public readonly Dictionary<Guid, RegisterVehicleRequestDto> vehicleById = new();
    public readonly HashSet<string> rendszamok = new();
}
