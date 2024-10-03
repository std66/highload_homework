using Npgsql;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using TomiSoft.HighLoad.App.Models.Api;

namespace TomiSoft.HighLoad.App.DataPersistence;

public class VehicleDataManager {
    private readonly NpgsqlDataSource dataSource;

    public VehicleDataManager(NpgsqlDataSource dataSource) {
        this.dataSource = dataSource;
    }

    public virtual async Task<Guid> RegisterVehicleAsync(RegisterVehicleRequestDto registerRequest, CancellationToken ct = default) {
        await using var connection = await dataSource.OpenConnectionAsync(ct);

        Guid id = Guid.NewGuid();

        // Tranzakció használata
        using var transaction = await connection.BeginTransactionAsync(ct);

        try {
            await InsertVehicleData(connection, registerRequest, id, transaction, ct);
            await InsertAdditionalData(connection, registerRequest, id, transaction, ct);

            // Tranzakció elkötelezése
            await transaction.CommitAsync(ct);
        }
        catch {
            // Ha hiba lép fel, visszagörgetjük a tranzakciót
            await transaction.RollbackAsync(ct);
            throw;
        }

        return id;
    }

    public virtual async IAsyncEnumerable<RegisteredVehicleDto> SearchVehicle(string query, [EnumeratorCancellation] CancellationToken ct = default) {
        await using var connection = await dataSource.OpenConnectionAsync(ct);

        const string sqlQuery = @"
            SELECT 
                jarmu.uuid,
                jarmu.rendszam,
                jarmu.tulajdonos,
                jarmu.forgalmi_ervenyes,
                jarmu.eredeti_adatok
            FROM jarmu
            WHERE jarmu.uuid IN (
                SELECT adatok.uuid
                FROM adatok
                WHERE adatok.adat LIKE '%' || @query || '%'
                GROUP BY adatok.uuid
            )
        ";

        await using var command = new NpgsqlCommand(sqlQuery, connection);
        command.Parameters.AddWithValue("query", query);

        await using var reader = await command.ExecuteReaderAsync(ct);

        while (await reader.ReadAsync(ct)) {
            yield return new RegisteredVehicleDto {
                Uuid = reader.GetGuid(0),
                Rendszam = reader.GetString(1),
                Tulajdonos = reader.GetString(2),
                ForgalmiErvenyes = DateOnly.FromDateTime(reader.GetDateTime(3)),
                Adatok = JsonSerializer.Deserialize<List<string>>(reader.GetString(4), AppJsonSerializerContext.Default.ListString) ?? []
            };
        }
    }

    public virtual async Task<long> GetCountOfVehiclesAsync(CancellationToken ct = default) {
        await using var connection = await dataSource.OpenConnectionAsync(ct);

        const string query = "SELECT COUNT(*) FROM jarmu";
        await using var command = new NpgsqlCommand(query, connection);

        var count = await command.ExecuteScalarAsync(ct);

        return (long?)count ?? 0L;
    }

    public virtual async Task<RegisteredVehicleDto?> GetVehicleById(Guid id, CancellationToken ct = default) {
        await using var connection = await dataSource.OpenConnectionAsync(ct);

        const string query = @"
            SELECT 
                j.uuid, 
                j.rendszam, 
                j.tulajdonos, 
                j.forgalmi_ervenyes, 
                j.eredeti_adatok
            FROM jarmu j
            WHERE j.uuid = @uuid
        ";

        await using var command = new NpgsqlCommand(query, connection);
        command.Parameters.AddWithValue("uuid", id);

        await using var reader = await command.ExecuteReaderAsync(ct);

        if (!await reader.ReadAsync(ct)) {
            return null; // Nincs ilyen jármű
        }

        var registeredVehicle = new RegisteredVehicleDto {
            Uuid = reader.GetGuid(0),
            Rendszam = reader.GetString(1),
            Tulajdonos = reader.GetString(2),
            ForgalmiErvenyes = DateOnly.FromDateTime(reader.GetDateTime(3)),
            Adatok = JsonSerializer.Deserialize(reader.GetString(4), AppJsonSerializerContext.Default.ListString) ?? []
        };

        return registeredVehicle;
    }

    private async Task InsertVehicleData(NpgsqlConnection connection, RegisterVehicleRequestDto registerRequest, Guid id, NpgsqlTransaction transaction, CancellationToken ct) {
        // SQL lekérdezés
        const string sql = @"
            INSERT INTO 
                jarmu (uuid, rendszam, tulajdonos, forgalmi_ervenyes, eredeti_adatok)
            VALUES 
                (@uuid, @rendszam, @tulajdonos, @forgalmi_ervenyes, @eredeti_adatok::json)
        ";

        // Lekérdezés előkészítése
        using (var command = new NpgsqlCommand(sql, connection, transaction)) {
            // Paraméterek hozzáadása
            command.Parameters.AddWithValue("uuid", id);
            command.Parameters.AddWithValue("rendszam", registerRequest.Rendszam);
            command.Parameters.AddWithValue("tulajdonos", registerRequest.Tulajdonos);
            command.Parameters.AddWithValue("forgalmi_ervenyes", registerRequest.ForgalmiErvenyes.GetValueOrDefault());
            command.Parameters.AddWithValue("eredeti_adatok", JsonSerializer.Serialize(registerRequest.Adatok, AppJsonSerializerContext.Default.ListString));

            // SQL lekérdezés végrehajtása
            await command.ExecuteNonQueryAsync(ct);
        }
    }

    private async Task InsertAdditionalData(NpgsqlConnection connection, RegisterVehicleRequestDto registerRequest, Guid id, NpgsqlTransaction transaction, CancellationToken ct) {
        // Tömeges beszúrás előkészítése az Adatok lista alapján
        var insertVehicleDataSql = new StringBuilder();
        insertVehicleDataSql.Append("INSERT INTO adatok (uuid, adat) VALUES ");

        // Paraméterek hozzáadása az SQL lekérdezéshez
        var parameters = new List<NpgsqlParameter>();

        int i = 0;
        for (; i < registerRequest.Adatok.Count; i++) {
            if (i > 0)
                insertVehicleDataSql.Append(", "); // Több beszúrás esetén vesszővel elválasztva

            insertVehicleDataSql.Append($"(@id, @adat{i})");
            parameters.Add(new NpgsqlParameter($"adat{i}", registerRequest.Adatok[i]));
        }

        //rendszám beszúrása
        insertVehicleDataSql.Append($",(@id, @adat{i}),");
        parameters.Add(new NpgsqlParameter($"adat{i}", registerRequest.Rendszam));

        //tulajdonos beszúrása
        insertVehicleDataSql.Append($"(@id, @adat{i + 1})");
        parameters.Add(new NpgsqlParameter($"adat{i + 1}", registerRequest.Tulajdonos));

        // vehicle_id paraméter hozzáadása
        parameters.Add(new NpgsqlParameter("id", id));

        // Lekérdezés végrehajtása
        using (var command = new NpgsqlCommand(insertVehicleDataSql.ToString(), connection, transaction)) {
            command.Parameters.AddRange(parameters.ToArray());
            await command.ExecuteNonQueryAsync(ct);
        }
    }
}
