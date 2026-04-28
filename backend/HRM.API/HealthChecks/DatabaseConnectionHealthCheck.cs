using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace HRM.API.HealthChecks;

public class DatabaseConnectionHealthCheck : IHealthCheck
{
    private readonly string _connectionString;

    public DatabaseConnectionHealthCheck(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' is missing.");
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync(cancellationToken);
            await using var command = new SqlCommand("SELECT 1", connection);
            await command.ExecuteScalarAsync(cancellationToken);

            return HealthCheckResult.Healthy("Database connection is healthy.");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy(
                description: "Database connection failed.",
                exception: ex);
        }
    }
}
