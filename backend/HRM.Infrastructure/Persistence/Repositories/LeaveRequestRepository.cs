using Dapper;
using HRM.Application.Common;
using HRM.Application.Common.Interfaces;
using HRM.Application.Leaves;
using HRM.Domain.Entities;
using Microsoft.Extensions.Configuration;
using System.Data;

namespace HRM.Infrastructure.Persistence.Repositories
{
    public class LeaveRequestRepository : GenericRepository<LeaveRequest>, ILeaveRequestRepository
    {
        // Stored Procedure Names
        private const string SP_GET_LEAVE_REQUESTS_WITH_EMPLOYEE_AND_DEPARTMENT = "sp_LeaveRequests_GetLeaveRequestsWithEmployeeAndDepartment";
        private const string SP_GET_LEAVE_REQUESTS_BY_DEPARTMENT_ID = "sp_LeaveRequests_GetLeaveRequestsByDepartmentId";
        private const string SP_GET_LEAVE_REQUESTS_BY_EMPLOYEE_ID = "sp_LeaveRequests_GetLeaveRequestsByEmployeeId";
        private const string SP_ADD_APPROVAL_LOG = "sp_ApprovalLogs_AddApprovalLog";

        public LeaveRequestRepository(IConfiguration configuration)
            : base(configuration)
        {
        }

        public async Task<PagedResult<LeaveRequestDto>> GetLeaveRequestsWithEmployeeAndDepartmentPagedAsync(byte? status, int pageNumber, int pageSize, CancellationToken cancellationToken)
        {
            using var conn = CreateConnection();

            var command = new CommandDefinition(
                SP_GET_LEAVE_REQUESTS_WITH_EMPLOYEE_AND_DEPARTMENT,
                new { Status = status, PageNumber = pageNumber, PageSize = pageSize },
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken);

            using var multi = await conn.QueryMultipleAsync(command);
            var totalCount = await multi.ReadSingleAsync<int>();
            var items = (await multi.ReadAsync<LeaveRequestDto>()).ToList();

            return new PagedResult<LeaveRequestDto>(items, totalCount, pageNumber, pageSize);
        }

        public async Task<PagedResult<LeaveRequestDto>> GetLeaveRequestsByDepartmentIdPagedAsync(int departmentId, byte? status, int pageNumber, int pageSize, CancellationToken cancellationToken)
        {
            using var conn = CreateConnection();

            var command = new CommandDefinition(
                SP_GET_LEAVE_REQUESTS_BY_DEPARTMENT_ID,
                new { DepartmentId = departmentId, Status = status, PageNumber = pageNumber, PageSize = pageSize },
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken);

            using var multi = await conn.QueryMultipleAsync(command);
            var totalCount = await multi.ReadSingleAsync<int>();
            var items = (await multi.ReadAsync<LeaveRequestDto>()).ToList();

            return new PagedResult<LeaveRequestDto>(items, totalCount, pageNumber, pageSize);
        }

        public async Task<PagedResult<LeaveRequestDto>> GetLeaveRequestsByEmployeeIdPagedAsync(int employeeId, byte? status, int pageNumber, int pageSize, CancellationToken cancellationToken)
        {
            using var conn = CreateConnection();

            var command = new CommandDefinition(
                SP_GET_LEAVE_REQUESTS_BY_EMPLOYEE_ID,
                new { EmployeeId = employeeId, Status = status, PageNumber = pageNumber, PageSize = pageSize },
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken);

            using var multi = await conn.QueryMultipleAsync(command);
            var totalCount = await multi.ReadSingleAsync<int>();
            var items = (await multi.ReadAsync<LeaveRequestDto>()).ToList();

            return new PagedResult<LeaveRequestDto>(items, totalCount, pageNumber, pageSize);
        }

        public async Task AddApprovalLogAsync(ApprovalLog approvalLog, CancellationToken cancellationToken)
        {
            using var conn = CreateConnection();

            var command = new CommandDefinition(
                SP_ADD_APPROVAL_LOG,
                new
                {
                    approvalLog.RequestId,
                    approvalLog.RequestType,
                    approvalLog.ApproverId,
                    Action = (byte)approvalLog.Action,
                    approvalLog.Comment,
                    approvalLog.Level,
                    CreatedAt = DateTime.UtcNow
                },
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken);

            await conn.ExecuteAsync(command);
        }

        public Task SaveChangesAsync(CancellationToken cancellationToken)
        {
            // Dapper is stateless, no DbContext to save changes
            return Task.CompletedTask;
        }
    }
}

