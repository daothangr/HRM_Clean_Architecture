using Dapper;
using HRM.Application.Common.Interfaces;
using HRM.Application.Departments;
using HRM.Domain.Entities;
using Microsoft.Extensions.Configuration;
using System.Data;

namespace HRM.Infrastructure.Persistence.Repositories;

public class DepartmentRepository : GenericRepository<Department>, IDepartmentRepository
{
    private const string SpGetDepartments = "sp_Departments_GetDepartments";
    private const string SpCheckDepartmentCodeExists = "sp_Departments_CheckDepartmentCodeExists";
    private const string SpCheckDepartmentExists = "sp_Departments_CheckDepartmentExists";
    private const string SpGetDepartmentById = "sp_Departments_GetDepartmentById";
    private const string SpDepartmentHasEmployees = "sp_Departments_CheckDepartmentHasEmployees";
    private const string SpAddDepartment = "sp_Departments_AddDepartment";

    public DepartmentRepository(IConfiguration configuration)
        : base(configuration)
    {
    }

    public async Task<List<DepartmentDto>> GetDepartmentsAsync(bool includeInactive, CancellationToken cancellationToken)
    {
        using var conn = CreateConnection();
        var command = new CommandDefinition(
            SpGetDepartments,
            new { IncludeInactive = includeInactive },
            commandType: CommandType.StoredProcedure,
            cancellationToken: cancellationToken);

        var departments = await conn.QueryAsync<DepartmentDto>(command);
        return departments.ToList();
    }

    public async Task<bool> DepartmentCodeExistsAsync(string code, int? excludeDepartmentId, CancellationToken cancellationToken)
    {
        var normalized = code.Trim();

        using var conn = CreateConnection();
        var command = new CommandDefinition(
            SpCheckDepartmentCodeExists,
            new { Code = normalized, ExcludeDepartmentId = excludeDepartmentId },
            commandType: CommandType.StoredProcedure,
            cancellationToken: cancellationToken);

        var isExists = await conn.ExecuteScalarAsync<int>(command);
        return isExists > 0;
    }

    public async Task<bool> DepartmentExistsAsync(int departmentId, CancellationToken cancellationToken)
    {
        using var conn = CreateConnection();
        var command = new CommandDefinition(
            SpCheckDepartmentExists,
            new { DepartmentId = departmentId },
            commandType: CommandType.StoredProcedure,
            cancellationToken: cancellationToken);

        var isExists = await conn.ExecuteScalarAsync<int>(command);
        return isExists > 0;
    }

    public async Task<Department?> GetDepartmentByIdAsync(int departmentId, CancellationToken cancellationToken)
    {
        using var conn = CreateConnection();
        var command = new CommandDefinition(
            SpGetDepartmentById,
            new { DepartmentId = departmentId },
            commandType: CommandType.StoredProcedure,
            cancellationToken: cancellationToken);

        return await conn.QueryFirstOrDefaultAsync<Department>(command);
    }

    public async Task<bool> DepartmentHasEmployeesAsync(int departmentId, CancellationToken cancellationToken)
    {
        using var conn = CreateConnection();
        var command = new CommandDefinition(
            SpDepartmentHasEmployees,
            new { DepartmentId = departmentId },
            commandType: CommandType.StoredProcedure,
            cancellationToken: cancellationToken);

        var hasEmployees = await conn.ExecuteScalarAsync<int>(command);
        return hasEmployees > 0;
    }

    public async Task AddDepartmentAsync(Department department, CancellationToken cancellationToken)
    {
        using var conn = CreateConnection();
        var command = new CommandDefinition(
            SpAddDepartment,
            new
            {
                department.Code,
                department.Name,
                Description = (string?)null,
                Status = (int)department.Status
            },
            commandType: CommandType.StoredProcedure,
            cancellationToken: cancellationToken);

        var newId = await conn.ExecuteScalarAsync<decimal>(command);
        department.Id = Convert.ToInt32(newId);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
