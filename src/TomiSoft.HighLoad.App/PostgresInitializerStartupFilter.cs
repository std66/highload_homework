
using Npgsql;

internal class PostgresInitializerStartupFilter(NpgsqlConnection connection, ILogger<PostgresInitializerStartupFilter> log) : IStartupFilter {
    public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next) {
        log.LogInformation("Begin initialization of Postgres connections...");

        int i = 0;
        bool success = false;

        do {
            try {
                if (connection.State != System.Data.ConnectionState.Open) {
                    connection.Open();
                    connection.Close();
                }

                success = true;
                log.LogInformation("Successful initialization");
            }
            catch {
                log.LogWarning("Failure, retrying...");
                i++;
                Thread.Sleep(150);
            }
        }
        while (!success && i < 40);

        return next;
    }
}