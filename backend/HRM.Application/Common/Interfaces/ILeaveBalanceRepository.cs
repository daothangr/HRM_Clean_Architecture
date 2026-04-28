using HRM.Domain.Entities;

namespace HRM.Application.Common.Interfaces;

public interface ILeaveBalanceRepository
{
    Task<LeaveBalance?> GetOrCreateLeaveBalanceAsync(int employeeId, int year, CancellationToken cancellationToken);
    Task<LeaveBalance?> GetLeaveBalanceAsync(int employeeId, int year, CancellationToken cancellationToken);
    Task UpdateLeaveBalanceAsync(LeaveBalance leaveBalance, CancellationToken cancellationToken);
}
