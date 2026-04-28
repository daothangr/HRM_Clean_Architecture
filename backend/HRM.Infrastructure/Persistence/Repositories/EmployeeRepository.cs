using System.Data;
using Dapper;
using HRM.Application.Common.Interfaces;
using HRM.Domain.Entities;
using Microsoft.Extensions.Configuration;

namespace HRM.Infrastructure.Persistence.Repositories;

public class EmployeeRepository : GenericRepository<Employee>, IEmployeeRepository
{
    // Stored Procedure Names
    private const string SP_GET_EMPLOYEES_WITH_DEPARTMENT_PAGED = "sp_Employees_GetEmployeesWithDepartmentPaged";
    private const string SP_GET_EMPLOYEES_BY_DEPARTMENT_PAGED = "sp_Employees_GetEmployeesByDepartmentPaged";
    private const string SP_GET_EMPLOYEE_BY_ID_WITH_DEPARTMENT_AND_ROLES = "sp_Employees_GetEmployeeByIdWithDepartmentAndRoles";
    private const string SP_GET_DEPARTMENT_ID_BY_EMPLOYEE_ID = "sp_Employees_GetDepartmentIdByEmployeeId";
    private const string SP_CHECK_EMAIL_EXISTS = "sp_Employees_CheckEmailExists";
    private const string SP_CHECK_EMPLOYEE_CODE_EXISTS = "sp_Employees_CheckEmployeeCodeExists";
    private const string SP_CHECK_EMPLOYEE_EXISTS = "sp_Employees_CheckEmployeeExists";
    private const string SP_ADD_EMPLOYEE = "sp_Employees_AddEmployee";
    private const string SP_ADD_USER_ROLE = "sp_Employees_AddUserRole";
    private const string SP_ENSURE_LEAVE_BALANCE = "sp_Employees_EnsureLeaveBalance";
    private const string SP_REPLACE_USER_ROLE = "sp_Employees_ReplaceUserRole";
    private const string SP_IS_EMPLOYEE_IN_DEPARTMENT = "sp_Employees_IsEmployeeInDepartment";
    private const string SP_GET_EMPLOYEE_WITH_ROLES_BY_EMAIL = "sp_Employees_GetEmployeeWithRolesByEmail";
    private const string SP_GET_EMPLOYEES_BY_ROLE_NAME = "sp_Employees_GetEmployeesByRoleName";
    private const string SP_GET_EMPLOYEE_ID_BY_CODE = "sp_Employees_GetEmployeeIdByEmployeeCode";

    public EmployeeRepository(IConfiguration configuration)
        : base(configuration)
    {
    }

    public async Task<(List<Employee> Employees, int TotalCount)> GetEmployeesWithDepartmentPagedAsync(
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken)
    {
        using var conn = CreateConnection();

        using var multi = await conn.QueryMultipleAsync(
            new CommandDefinition(
                SP_GET_EMPLOYEES_WITH_DEPARTMENT_PAGED,
                new
                {
                    PageNumber = Math.Max(1, pageNumber),
                    PageSize = Math.Clamp(pageSize, 1, 100)
                },
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken
            )
        );

        var employees = (await multi.ReadAsync<Employee>()).ToList();

        var departmentDict = (await multi.ReadAsync<Department>())
            .ToDictionary(d => d.Id);

        var totalCount = await multi.ReadFirstAsync<int>();

        foreach (var emp in employees)
        {
            departmentDict.TryGetValue(emp.DepartmentId, out var department);
            emp.Department = department;
        }

        return (employees, totalCount);
    }


    public async Task<(List<Employee> Employees, int TotalCount)> GetEmployeesByDepartmentPagedAsync(
        int departmentId,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken)
    {
        using var conn = CreateConnection();

        using var multi = await conn.QueryMultipleAsync(
            new CommandDefinition(
                SP_GET_EMPLOYEES_BY_DEPARTMENT_PAGED,
                new
                {
                    DepartmentId = departmentId,
                    PageNumber = pageNumber,
                    PageSize = pageSize
                },
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken
            )
        );

        var employees = multi.Read<Employee, Department, Employee>(
            (emp, dept) =>
            {
                emp.Department = dept;
                return emp;
            },
            splitOn: "DeptId"
        ).ToList();

        var totalCount = await multi.ReadFirstAsync<int>();

        return (employees, totalCount);
    }

    public async Task<Employee?> GetEmployeeByIdWithDepartmentAndRolesAsync(
        int employeeId,
        CancellationToken cancellationToken)
    {
        using var conn = CreateConnection();

        using var multi = await conn.QueryMultipleAsync(
            new CommandDefinition(
                SP_GET_EMPLOYEE_BY_ID_WITH_DEPARTMENT_AND_ROLES,
                new { EmployeeId = employeeId },
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken
            )
        );

        // =========================
        // 1. Employee
        // =========================
        var employee = await multi.ReadFirstOrDefaultAsync<Employee>();

        if (employee is null)
            return null;

        // =========================
        // 2. Department
        // =========================
        var department = await multi.ReadFirstOrDefaultAsync<Department>();
        employee.Department = department!;

        // =========================
        // 3. Roles
        // =========================
        var roles = (await multi.ReadAsync<Role>()).ToList();

        employee.UserRoles = roles.Select(r => new UserRole
        {
            UserId = employee.Id,
            RoleId = r.Id,
            Role = r
        }).ToList();

        return employee;
    }

    public async Task<int?> GetDepartmentIdByEmployeeIdAsync(int employeeId, CancellationToken cancellationToken)
    {
        using var conn = CreateConnection();

        return await conn.QueryFirstOrDefaultAsync<int?>(
            new CommandDefinition(
                SP_GET_DEPARTMENT_ID_BY_EMPLOYEE_ID,
                new { EmployeeId = employeeId },
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken
            )
        );
    }

    public async Task<int?> GetEmployeeIdByEmployeeCodeAsync(string employeeCode, CancellationToken cancellationToken)
    {
        var normalized = employeeCode.Trim();

        using var conn = CreateConnection();

        return await conn.QueryFirstOrDefaultAsync<int?>(
            new CommandDefinition(
                SP_GET_EMPLOYEE_ID_BY_CODE,
                new { EmployeeCode = normalized },
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken
            )
        );
    }

    public async Task<bool> EmailExistsAsync(string email, int? excludeEmployeeId, CancellationToken cancellationToken)
    {
        var normalized = email.Trim();

        using var conn = CreateConnection();

        var result = await conn.ExecuteScalarAsync<int>(
            new CommandDefinition(
                SP_CHECK_EMAIL_EXISTS,
                new
                {
                    Email = normalized,
                    ExcludeEmployeeId = excludeEmployeeId
                },
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken
            )
        );

        return result == 1;
    }

    public async Task<bool> EmployeeCodeExistsAsync(string employeeCode, CancellationToken cancellationToken)
    {
        var normalized = employeeCode.Trim();

        using var conn = CreateConnection();

        var result = await conn.ExecuteScalarAsync<int>(
            new CommandDefinition(
                SP_CHECK_EMPLOYEE_CODE_EXISTS,
                new
                {
                    EmployeeCode = normalized
                },
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken
            )
        );

        return result == 1;
    }

    public async Task<bool> EmployeeExistsAsync(int employeeId, CancellationToken cancellationToken)
    {
        using var conn = CreateConnection();

        var result = await conn.ExecuteScalarAsync<int>(
            new CommandDefinition(
                SP_CHECK_EMPLOYEE_EXISTS,
                new
                {
                    EmployeeId = employeeId
                },
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken
            )
        );

        return result == 1;
    }
public async Task AddEmployeeAsync(Employee employee, CancellationToken cancellationToken)
{
    using var conn = CreateConnection();

    var newId = await conn.ExecuteScalarAsync<int>(
        new CommandDefinition(
            SP_ADD_EMPLOYEE,
            new
            {
                employee.EmployeeCode,
                employee.FullName,
                employee.Email,
                employee.Phone,
                employee.DepartmentId,
                employee.Position,
                HireDate = employee.HireDate?.Date,
                employee.PasswordHash   
            },
            commandType: CommandType.StoredProcedure,
            cancellationToken: cancellationToken
        )
    );
    employee.Id = newId; 
}

    public async Task AddUserRoleAsync(int userId, int roleId, CancellationToken cancellationToken)
    {
        using var conn = CreateConnection();

        await conn.ExecuteAsync(
            new CommandDefinition(
                SP_ADD_USER_ROLE,
                new
                {
                    UserId = userId,
                    RoleId = roleId
                },
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken
            )
        );
    }

    public async Task EnsureLeaveBalanceForCurrentYearAsync(int employeeId, CancellationToken cancellationToken)
    {
        using var conn = CreateConnection();

        await conn.ExecuteAsync(
            new CommandDefinition(
                SP_ENSURE_LEAVE_BALANCE,
                new { EmployeeId = employeeId },
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken
            )
        );
    }

    public async Task ReplaceUserRoleAsync(Employee employee, int roleId, CancellationToken cancellationToken)
    {
        using var conn = CreateConnection();

        await conn.ExecuteAsync(
            new CommandDefinition(
                SP_REPLACE_USER_ROLE,
                new
                {
                       UserId = employee.Id,
                        RoleId = roleId
                },
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken
            )
        );

        // update in-memory entity (optional but good practice)
        employee.UserRoles.Clear();
        employee.UserRoles.Add(new UserRole
        {
            UserId = employee.Id,
            RoleId = roleId
        });
    }
    public Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    public async Task<bool> IsEmployeeInDepartmentAsync(int employeeId, int departmentId, CancellationToken cancellationToken)
    {
        using var conn = CreateConnection();

        var result = await conn.ExecuteScalarAsync<int>(
            new CommandDefinition(
                SP_IS_EMPLOYEE_IN_DEPARTMENT,
                new { EmployeeId = employeeId, DepartmentId = departmentId },
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken
            )
        );

        return result == 1;
    }

    public async Task<Employee?> GetEmployeeWithRolesByEmailAsync(
        string email,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(email))
            return null;

        using var conn = CreateConnection();

        if (conn.State == ConnectionState.Closed)
            conn.Open();

        var command = new CommandDefinition(
            SP_GET_EMPLOYEE_WITH_ROLES_BY_EMAIL,
            new { Email = email },
            commandType: CommandType.StoredProcedure,
            cancellationToken: cancellationToken
        );

        using var multi = await conn.QueryMultipleAsync(command);

        // 1. Lấy employee
        var employee = multi.ReadFirstOrDefault<Employee>();

        if (employee == null)
            return null;

        // 2. Lấy roles
        var roles = multi.Read<Role>().ToList();

        // 3. Map vào UserRoles (không set User để tránh circular reference)
        employee.UserRoles = roles.Select(r => new UserRole
        {
            UserId = employee.Id,
            RoleId = r.Id,
            Role = r
        }).ToList();

        return employee;
    }

    public async Task<List<Employee>> GetEmployeesByRoleNameAsync(string roleName, CancellationToken cancellationToken)
    {
        using var conn = CreateConnection();

        using var multi = await conn.QueryMultipleAsync(
            new CommandDefinition(
                SP_GET_EMPLOYEES_BY_ROLE_NAME,
                new { RoleName = roleName },
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken
            )
        );

        var employees = (await multi.ReadAsync<Employee>()).ToList();
        var departments = (await multi.ReadAsync<Department>()).ToList();

        employees.ForEach(emp =>
        {
            emp.Department = departments.FirstOrDefault(d => d.Id == emp.DepartmentId)
                ?? new Department { Id = emp.DepartmentId, Name = string.Empty };
        });

        return employees;
    }
}


