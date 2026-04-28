using HRM.Application.Common.Interfaces;
using HRM.Domain.Entities;
using HRM.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Transactions;

namespace HRM.Infrastructure.Persistence;

public class ApplicationDbContext : DbContext, IApplicationDbContext, IUnitOfWork
{
    private TransactionScope? _transactionScope;

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public async Task BeginTransactionAsync()
    {
        if (_transactionScope is not null)
        {
            return;
        }

        _transactionScope = new TransactionScope(
            TransactionScopeOption.Required,
            new TransactionOptions
            {
                IsolationLevel = IsolationLevel.ReadCommitted
            },
            TransactionScopeAsyncFlowOption.Enabled);

        await Task.CompletedTask;
    }

    public async Task CommitAsync()
    {
        if (_transactionScope is null)
        {
            return;
        }

        _transactionScope.Complete();
        _transactionScope.Dispose();
        _transactionScope = null;
        await Task.CompletedTask;
    }

    public async Task RollbackAsync()
    {
        if (_transactionScope is null)
        {
            return;
        }

        _transactionScope.Dispose();
        _transactionScope = null;
        await Task.CompletedTask;
    }

    public DbSet<Department> Departments => Set<Department>();
    public DbSet<Employee> Employees => Set<Employee>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<Permission> Permissions => Set<Permission>();
    public DbSet<RolePermission> RolePermissions => Set<RolePermission>();
    public DbSet<UserRole> UserRoles => Set<UserRole>();
    public DbSet<LeaveRequest> LeaveRequests => Set<LeaveRequest>();
    public DbSet<ApprovalWorkflow> ApprovalWorkflows => Set<ApprovalWorkflow>();
    public DbSet<ApprovalLog> ApprovalLogs => Set<ApprovalLog>();
    public DbSet<LeaveBalance> LeaveBalances => Set<LeaveBalance>();
    public DbSet<Attendance> Attendances => Set<Attendance>();
    public DbSet<OvertimeRequest> OvertimeRequests => Set<OvertimeRequest>();
    public DbSet<ActivityLog> ActivityLogs => Set<ActivityLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<UserRole>(e =>
        {
            e.HasKey(ur => new { ur.UserId, ur.RoleId });
            e.HasOne(ur => ur.User).WithMany(u => u.UserRoles).HasForeignKey(ur => ur.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            e.HasOne(ur => ur.Role).WithMany(r => r.UserRoles).HasForeignKey(ur => ur.RoleId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<RolePermission>(e =>
        {
            e.HasKey(rp => new { rp.RoleId, rp.PermissionId });
            e.HasOne(rp => rp.Role).WithMany(r => r.RolePermissions).HasForeignKey(rp => rp.RoleId);
            e.HasOne(rp => rp.Permission).WithMany(p => p.RolePermissions).HasForeignKey(rp => rp.PermissionId);
        });

        modelBuilder.Entity<Department>(e =>
        {
            e.HasOne(d => d.ParentDepartment).WithMany(d => d.ChildDepartments)
                .HasForeignKey(d => d.ParentDepartmentId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(d => d.DepartmentHead).WithMany().HasForeignKey(d => d.DepartmentHeadId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Employee>(e =>
        {
            e.HasIndex(x => x.Email).IsUnique();
            e.HasIndex(x => x.EmployeeCode).IsUnique();
            e.HasOne(x => x.Department).WithMany(d => d.Employees).HasForeignKey(x => x.DepartmentId);
            e.HasOne(x => x.Manager).WithMany(x => x.DirectReports).HasForeignKey(x => x.ManagerId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<LeaveRequest>(e =>
        {
            e.HasOne(x => x.Employee).WithMany(x => x.LeaveRequests).HasForeignKey(x => x.EmployeeId);
        });

        modelBuilder.Entity<ApprovalLog>(e =>
        {
            e.HasOne(x => x.Approver).WithMany().HasForeignKey(x => x.ApproverId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<LeaveBalance>(e =>
        {
            e.HasIndex(x => new { x.EmployeeId, x.Year }).IsUnique();
            e.HasOne(x => x.Employee).WithMany(x => x.LeaveBalances).HasForeignKey(x => x.EmployeeId);
        });

        modelBuilder.Entity<Attendance>(e =>
        {
            e.HasIndex(x => new { x.EmployeeId, x.Date }).IsUnique();
            e.HasOne(x => x.Employee).WithMany().HasForeignKey(x => x.EmployeeId);
        });

        modelBuilder.Entity<OvertimeRequest>(e =>
        {
            e.HasOne(x => x.Employee).WithMany(x => x.OvertimeRequests).HasForeignKey(x => x.EmployeeId);
        });

        modelBuilder.Entity<ActivityLog>(e =>
        {
            e.HasOne(x => x.User).WithMany().HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<ApprovalWorkflow>(e =>
        {
            e.HasOne(x => x.Department).WithMany().HasForeignKey(x => x.DepartmentId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        base.OnModelCreating(modelBuilder);
    }
}
