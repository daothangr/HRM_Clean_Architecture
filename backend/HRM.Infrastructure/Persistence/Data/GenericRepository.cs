using Dapper;
using HRM.Application.Common.Interfaces.Repositories;
using HRM.Domain.Interfaces;
using HRM.Infrastructure.Persistence.Data;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Data;
using System.Reflection;
using System.Collections;

namespace HRM.Infrastructure.Persistence.Repositories;

public class GenericRepository<TEntity> : IGenericRepository<TEntity> where TEntity : class
{
    private readonly string _connectionString;

    public GenericRepository(IConfiguration configuration)
    {
        DapperTypeHandlers.Register();
        _connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' was not found.");
    }

    protected IDbConnection CreateConnection() => new SqlConnection(_connectionString);

    public async Task<TEntity?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        using var connection = CreateConnection();
        var sql = $"SELECT * FROM {GetTableName()} WHERE [Id] = @Id";
        var command = new CommandDefinition(sql, new { Id = id }, cancellationToken: cancellationToken);
        return await connection.QueryFirstOrDefaultAsync<TEntity>(command);
    }

    public async Task<List<TEntity>> ListAsync(CancellationToken cancellationToken = default)
    {
        using var connection = CreateConnection();
        var sql = $"SELECT * FROM {GetTableName()}";
        var command = new CommandDefinition(sql, cancellationToken: cancellationToken);
        var result = await connection.QueryAsync<TEntity>(command);
        return result.ToList();
    }

    public async Task AddAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        using var connection = CreateConnection();

        var properties = GetAllProperties()
            .Where(p => !string.Equals(p.Name, "Id", StringComparison.OrdinalIgnoreCase))
            .ToArray();
        var columns = string.Join(", ", properties.Select(p => $"[{p.Name}]"));
        var values = string.Join(", ", properties.Select(p => $"@{p.Name}"));

        var parameters = new DynamicParameters();
        foreach (var property in properties)
        {
            parameters.Add(property.Name, property.GetValue(entity));
        }

        var sql = $@"
            INSERT INTO {GetTableName()} ({columns})
            OUTPUT INSERTED.[Id]
            VALUES ({values});";

        var command = new CommandDefinition(sql, parameters, cancellationToken: cancellationToken);
        var insertedId = await connection.QuerySingleAsync<int>(command);

        var idProperty = GetIdProperty();
        if (idProperty?.CanWrite == true)
        {
            idProperty.SetValue(entity, Convert.ChangeType(insertedId, idProperty.PropertyType));
        }
    }

    public async Task UpdateAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        using var connection = CreateConnection();

        var idProperty = GetIdProperty() ?? throw new InvalidOperationException($"{typeof(TEntity).Name} must have Id property.");
        var id = idProperty.GetValue(entity) ?? throw new InvalidOperationException($"{typeof(TEntity).Name}.Id cannot be null.");

        var properties = GetAllProperties()
            .Where(p => !string.Equals(p.Name, "Id", StringComparison.OrdinalIgnoreCase))
            .Where(p => !string.Equals(p.Name, "CreatedAt", StringComparison.OrdinalIgnoreCase))
            .ToArray();
        var setClause = string.Join(", ", properties.Select(p => $"[{p.Name}] = @{p.Name}"));
        var sql = $"UPDATE {GetTableName()} SET {setClause} WHERE [Id] = @Id";

        var parameters = new DynamicParameters();
        foreach (var property in properties)
        {
            parameters.Add(property.Name, property.GetValue(entity));
        }
        parameters.Add("Id", id);

        var command = new CommandDefinition(sql, parameters, cancellationToken: cancellationToken);
        var affectedRows = await connection.ExecuteAsync(command);
        if (affectedRows == 0)
        {
            throw new KeyNotFoundException($"{typeof(TEntity).Name} not found.");
        }
    }

    public async Task RemoveAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        using var connection = CreateConnection();

        var idProperty = GetIdProperty() ?? throw new InvalidOperationException($"{typeof(TEntity).Name} must have Id property.");
        var id = idProperty.GetValue(entity) ?? throw new InvalidOperationException($"{typeof(TEntity).Name}.Id cannot be null.");

        var sql = $"DELETE FROM {GetTableName()} WHERE [Id] = @Id";
        var command = new CommandDefinition(sql, new { Id = id }, cancellationToken: cancellationToken);
        await connection.ExecuteAsync(command);
    }

    private static string GetTableName() => $"[dbo].[{typeof(TEntity).Name}s]";

    private static PropertyInfo? GetIdProperty() => typeof(TEntity).GetProperty("Id");

    private static PropertyInfo[] GetAllProperties() =>
        typeof(TEntity)
            .GetProperties()
            .Where(IsPersistedProperty)
            .ToArray();

    private static bool IsPersistedProperty(PropertyInfo property)
    {
        if (property.GetIndexParameters().Length > 0)
            return false;

        var type = property.PropertyType;

        if (type == typeof(string) || type == typeof(byte[]))
            return true;

        if (typeof(IEnumerable).IsAssignableFrom(type) && type != typeof(byte[]))
            return false;

        if (type.IsValueType)
            return true;

        return false;
    }
}
