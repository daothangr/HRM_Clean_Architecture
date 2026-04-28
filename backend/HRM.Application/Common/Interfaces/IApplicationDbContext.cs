using HRM.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace HRM.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    DbSet<Department> Departments { get; }
    DbSet<Employee> Employees { get; }
    DbSet<Role> Roles { get; }
    DbSet<Permission> Permissions { get; }
    DbSet<RolePermission> RolePermissions { get; }
    DbSet<UserRole> UserRoles { get; }
    DbSet<LeaveRequest> LeaveRequests { get; }
    DbSet<ApprovalWorkflow> ApprovalWorkflows { get; }
    DbSet<ApprovalLog> ApprovalLogs { get; }
    DbSet<LeaveBalance> LeaveBalances { get; }
    DbSet<Attendance> Attendances { get; }
    DbSet<OvertimeRequest> OvertimeRequests { get; }
    DbSet<ActivityLog> ActivityLogs { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
