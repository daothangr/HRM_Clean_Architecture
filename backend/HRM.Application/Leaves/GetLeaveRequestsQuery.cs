using HRM.Application.Common.Constants;
using HRM.Application.Common;
using HRM.Application.Common.Interfaces;
using MediatR;

namespace HRM.Application.Leaves;

public record GetLeaveRequestsQuery(byte? Status = null, int PageNumber = 1, int PageSize = 10) : IRequest<PagedResult<LeaveRequestDto>>;

public class GetLeaveRequestsQueryHandler : IRequestHandler<GetLeaveRequestsQuery, PagedResult<LeaveRequestDto>>
{
    private readonly ILeaveRequestRepository _leaveRequestRepository;
    private readonly IEmployeeRepository _employeeRepository;
    private readonly ICurrentUserService _current;

    public GetLeaveRequestsQueryHandler(
        ILeaveRequestRepository leaveRequestRepository,
        IEmployeeRepository employeeRepository,
        ICurrentUserService current)
    {
        _leaveRequestRepository = leaveRequestRepository;
        _employeeRepository = employeeRepository;
        _current = current;
    }

    public async Task<PagedResult<LeaveRequestDto>> Handle(GetLeaveRequestsQuery request, CancellationToken cancellationToken)
    {
        var pageNumber = request.PageNumber < 1 ? 1 : request.PageNumber;
        var pageSize = request.PageSize < 1 ? 10 : request.PageSize;

        // ADMIN / DIRECTOR
        if (_current.IsInRole(SystemRoles.Admin) ||
            _current.IsInRole(SystemRoles.Director))
        {
            return await _leaveRequestRepository.GetLeaveRequestsWithEmployeeAndDepartmentPagedAsync(request.Status, pageNumber, pageSize, cancellationToken);
        }

        // MANAGER
        if (_current.IsInRole(SystemRoles.Manager) && _current.UserId is int mgrId)
        {
            var deptId = await _employeeRepository.GetDepartmentIdByEmployeeIdAsync(mgrId, cancellationToken);
            if (!deptId.HasValue)
            {
                throw new UnauthorizedAccessException();
            }

            return await _leaveRequestRepository.GetLeaveRequestsByDepartmentIdPagedAsync(deptId.Value, request.Status, pageNumber, pageSize, cancellationToken);
        }

        // EMPLOYEE NORMAL
        if (_current.UserId is int uid)
        {
            return await _leaveRequestRepository.GetLeaveRequestsByEmployeeIdPagedAsync(uid, request.Status, pageNumber, pageSize, cancellationToken);
        }

        throw new UnauthorizedAccessException();
    }
}
