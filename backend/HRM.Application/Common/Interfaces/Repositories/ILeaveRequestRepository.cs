using HRM.Application.Common.Interfaces.Repositories;
using HRM.Application.Common;
using HRM.Application.Leaves;
using HRM.Domain.Entities;

namespace HRM.Application.Common.Interfaces;

public interface ILeaveRequestRepository : IGenericRepository<LeaveRequest>
{
    Task<PagedResult<LeaveRequestDto>> GetLeaveRequestsWithEmployeeAndDepartmentPagedAsync(byte? status, int pageNumber, int pageSize, CancellationToken cancellationToken);
    Task<PagedResult<LeaveRequestDto>> GetLeaveRequestsByDepartmentIdPagedAsync(int departmentId, byte? status, int pageNumber, int pageSize, CancellationToken cancellationToken);
    Task<PagedResult<LeaveRequestDto>> GetLeaveRequestsByEmployeeIdPagedAsync(int employeeId, byte? status, int pageNumber, int pageSize, CancellationToken cancellationToken);
    Task AddApprovalLogAsync(ApprovalLog approvalLog, CancellationToken cancellationToken);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
