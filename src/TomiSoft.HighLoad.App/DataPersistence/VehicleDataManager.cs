﻿using Npgsql;
using System.Text;
using System.Text.Json;
using TomiSoft.HighLoad.App.Models.Api;

namespace TomiSoft.HighLoad.App.DataPersistence;

public class VehicleDataManager {
    private readonly NpgsqlConnection connection;

    public VehicleDataManager(NpgsqlConnection connection) {
        this.connection = connection;
    }

    public async Task<Guid> RegisterVehicleAsync(RegisterVehicleRequestDto registerRequest) {
        await VerifyConnectionAsync();

        Guid id = Guid.NewGuid();

        // Tranzakció használata
        using var transaction = await connection.BeginTransactionAsync();

        try {
            await InsertVehicleData(registerRequest, id, transaction);
            await InsertAdditionalData(registerRequest, id, transaction);

            // Tranzakció elkötelezése
            await transaction.CommitAsync();
        }
        catch {
            // Ha hiba lép fel, visszagörgetjük a tranzakciót
            await transaction.RollbackAsync();
            throw;
        }

        await connection.CloseAsync();

        return id;
    }

    public async Task<int> GetCountOfVehiclesAsync() {
        await VerifyConnectionAsync();

        const string query = "SELECT COUNT(*) FROM jarmu";
        await using var command = new NpgsqlCommand(query, connection);

        var count = (await command.ExecuteScalarAsync()) as int?;

        await connection.CloseAsync();

        return count ?? 0;
    }

    private async Task VerifyConnectionAsync() {
        if (connection.State == System.Data.ConnectionState.Open) {
            return;
        }

        await connection.OpenAsync();
    }

    public async Task<RegisteredVehicleDto?> GetVehicleById(Guid id) {
        await VerifyConnectionAsync();

        const string query = @"
            SELECT 
                j.uuid, 
                j.rendszam, 
                j.tulajdonos, 
                j.forgalmi_ervenyes, 
                COALESCE(json_agg(a.adat), '[]'::json) AS adatok
            FROM jarmu j
            LEFT JOIN adatok a ON j.uuid = a.uuid
            WHERE j.uuid = @uuid
            GROUP BY j.uuid
        ";

        await using var command = new NpgsqlCommand(query, connection);
        command.Parameters.AddWithValue("uuid", id);

        await using var reader = await command.ExecuteReaderAsync();

        if (!await reader.ReadAsync()) {
            await connection.CloseAsync();
            return null; // Nincs ilyen jármű
        }

        var registeredVehicle = new RegisteredVehicleDto {
            Uuid = reader.GetGuid(0),
            Rendszam = reader.GetString(1),
            Tulajdonos = reader.GetString(2),
            ForgalmiErvenyes = DateOnly.FromDateTime(reader.GetDateTime(3)),
            Adatok = JsonSerializer.Deserialize(reader.GetString(4), AppJsonSerializerContext.Default.ListString) ?? []
        };

        await connection.CloseAsync();

        return registeredVehicle;
    }

    private async Task InsertVehicleData(RegisterVehicleRequestDto registerRequest, Guid id, NpgsqlTransaction transaction) {
        // SQL lekérdezés
        const string sql = @"
            INSERT INTO 
                jarmu (uuid, rendszam, tulajdonos, forgalmi_ervenyes)
            VALUES 
                (@uuid, @rendszam, @tulajdonos, @forgalmi_ervenyes)
        ";

        // Lekérdezés előkészítése
        using (var command = new NpgsqlCommand(sql, connection, transaction)) {
            // Paraméterek hozzáadása
            command.Parameters.AddWithValue("uuid", id);
            command.Parameters.AddWithValue("rendszam", registerRequest.Rendszam);
            command.Parameters.AddWithValue("tulajdonos", registerRequest.Tulajdonos);
            command.Parameters.AddWithValue("forgalmi_ervenyes", registerRequest.ForgalmiErvenyes.HasValue ? (object)registerRequest.ForgalmiErvenyes.Value : DBNull.Value);

            // SQL lekérdezés végrehajtása
            await command.ExecuteNonQueryAsync();
        }
    }

    private async Task InsertAdditionalData(RegisterVehicleRequestDto registerRequest, Guid id, NpgsqlTransaction transaction) {
        // Tömeges beszúrás előkészítése az Adatok lista alapján
        var insertVehicleDataSql = new StringBuilder();
        insertVehicleDataSql.Append("INSERT INTO adatok (uuid, adat) VALUES ");

        // Paraméterek hozzáadása az SQL lekérdezéshez
        var parameters = new List<NpgsqlParameter>();
        for (int i = 0; i < registerRequest.Adatok.Count; i++) {
            if (i > 0)
                insertVehicleDataSql.Append(", "); // Több beszúrás esetén vesszővel elválasztva

            insertVehicleDataSql.Append($"(@id, @adat{i})");
            parameters.Add(new NpgsqlParameter($"adat{i}", registerRequest.Adatok[i]));
        }

        // vehicle_id paraméter hozzáadása
        parameters.Add(new NpgsqlParameter("id", id));

        // Lekérdezés végrehajtása
        using (var command = new NpgsqlCommand(insertVehicleDataSql.ToString(), connection, transaction)) {
            command.Parameters.AddRange(parameters.ToArray());
            await command.ExecuteNonQueryAsync();
        }
    }
}
