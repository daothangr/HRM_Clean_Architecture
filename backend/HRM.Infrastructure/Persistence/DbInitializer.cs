using HRM.Application.Common.Constants;
using HRM.Application.Common.Interfaces;
using HRM.Domain.Entities;
using HRM.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace HRM.Infrastructure.Persistence;

public static class DbInitializer
{
    public static async Task SeedAsync(IServiceProvider services, CancellationToken cancellationToken = default)
    {
        await using var scope = services.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
        var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger(nameof(DbInitializer));

        logger.LogInformation("Seeding database...");

        async Task<Role> EnsureRoleAsync(string name, string description)
        {
            var role = await context.Roles.FirstOrDefaultAsync(r => r.Name == name, cancellationToken);
            if (role is not null)
            {
                role.Description ??= description;
                role.IsSystem = true;
                return role;
            }

            role = new Role { Name = name, Description = description, IsSystem = true };
            context.Roles.Add(role);
            await context.SaveChangesAsync(cancellationToken);
            return role;
        }

        async Task<Department> EnsureDepartmentAsync(string code, string name)
        {
            var department = await context.Departments.FirstOrDefaultAsync(d => d.Code == code, cancellationToken);
            if (department is not null)
            {
                department.Name = name;
                if (department.Status == DepartmentStatus.Inactive)
                    department.Status = DepartmentStatus.Active;
                return department;
            }

            department = new Department
            {
                Code = code,
                Name = name,
                Status = DepartmentStatus.Active,
                CreatedAt = DateTime.UtcNow
            };
            context.Departments.Add(department);
            await context.SaveChangesAsync(cancellationToken);
            return department;
        }

        async Task<Employee> EnsureEmployeeAsync(
            string employeeCode,
            string fullName,
            string email,
            int departmentId,
            string position,
            DateTime hireDate,
            string password,
            EmployeeStatus status)
        {
            var employee = await context.Employees.FirstOrDefaultAsync(e => e.EmployeeCode == employeeCode || e.Email == email, cancellationToken);
            if (employee is not null)
            {
                employee.FullName = fullName;
                employee.Email = email;
                employee.DepartmentId = departmentId;
                employee.Position = position;
                employee.HireDate = hireDate;
                employee.Status = status;
                employee.IsActive = true;
                return employee;
            }

            employee = new Employee
            {
                EmployeeCode = employeeCode,
                FullName = fullName,
                Email = email,
                DepartmentId = departmentId,
                Position = position,
                HireDate = hireDate,
                Status = status,
                IsActive = true,
                PasswordHash = passwordHasher.Hash(password),
                CreatedAt = DateTime.UtcNow
            };
            context.Employees.Add(employee);
            await context.SaveChangesAsync(cancellationToken);
            return employee;
        }

        var adminRole = await EnsureRoleAsync(SystemRoles.Admin, "HR / System admin");
        var directorRole = await EnsureRoleAsync(SystemRoles.Director, "Director");
        var managerRole = await EnsureRoleAsync(SystemRoles.Manager, "Head of department");
        var employeeRole = await EnsureRoleAsync(SystemRoles.Employee, "Employee");

        var deptIt = await EnsureDepartmentAsync("IT", "Information Technology");
        var deptHr = await EnsureDepartmentAsync("HR", "Human Resources");

        var pwd = "Admin@123";
        var director = await EnsureEmployeeAsync("DIR001", "Demo Director", "director@company.com", deptIt.Id, "Director", new DateTime(2020, 1, 1), pwd, EmployeeStatus.Active);
        var manager = await EnsureEmployeeAsync("MGR001", "Demo Manager", "manager@company.com", deptIt.Id, "IT Manager", new DateTime(2021, 1, 1), pwd, EmployeeStatus.Active);
        var employee = await EnsureEmployeeAsync("EMP001", "Demo Employee", "employee@company.com", deptIt.Id, "Developer", new DateTime(2022, 1, 1), pwd, EmployeeStatus.Active);
        var hrAdmin = await EnsureEmployeeAsync("HR001", "Demo HR Admin", "admin@hr.com", deptHr.Id, "HR Specialist", new DateTime(2019, 1, 1), pwd, EmployeeStatus.Active);

        await context.SaveChangesAsync(cancellationToken);

        manager.ManagerId = director.Id;
        employee.ManagerId = manager.Id;
        deptIt.DepartmentHeadId = manager.Id;

        var userRolePairs = new[]
        {
            new UserRole { UserId = director.Id, RoleId = directorRole.Id },
            new UserRole { UserId = manager.Id, RoleId = managerRole.Id },
            new UserRole { UserId = employee.Id, RoleId = employeeRole.Id },
            new UserRole { UserId = hrAdmin.Id, RoleId = adminRole.Id }
        };

        foreach (var userRole in userRolePairs)
        {
            var exists = await context.UserRoles.AnyAsync(x => x.UserId == userRole.UserId && x.RoleId == userRole.RoleId, cancellationToken);
            if (!exists)
                context.UserRoles.Add(userRole);
        }

        var year = DateTime.UtcNow.Year;
        var leaveBalanceEmployees = new[] { employee.Id, manager.Id, director.Id, hrAdmin.Id };
        foreach (var employeeId in leaveBalanceEmployees)
        {
            var exists = await context.LeaveBalances.AnyAsync(x => x.EmployeeId == employeeId && x.Year == year, cancellationToken);
            if (!exists)
            {
                context.LeaveBalances.Add(new LeaveBalance { EmployeeId = employeeId, Year = year });
            }
        }

        await context.SaveChangesAsync(cancellationToken);
        logger.LogInformation("Database seed completed. Demo password for all accounts: {Pwd}", pwd);
    }
}
