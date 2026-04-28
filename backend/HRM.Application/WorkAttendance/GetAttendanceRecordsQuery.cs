using HRM.Application.Common.Constants;
using HRM.Application.Common;
using HRM.Application.Common.Interfaces;
using HRM.Application.Common.Interfaces.Repositories;
using MediatR;

namespace HRM.Application.WorkAttendance;

public record GetAttendanceRecordsQuery(DateTime From, DateTime To, string? EmployeeCode, int PageNumber = 1, int PageSize = 10) : IRequest<PagedResult<AttendanceRecordDto>>;

public class GetAttendanceRecordsQueryHandler : IRequestHandler<GetAttendanceRecordsQuery, PagedResult<AttendanceRecordDto>>
{
    private readonly IAttendanceRepository _attendanceRepo;
    private readonly IEmployeeRepository _employeeRepo; 
    private readonly ICurrentUserService _current;

    public GetAttendanceRecordsQueryHandler(
        IEmployeeRepository employeeRepo,
        IAttendanceRepository attendanceRepo,
        ICurrentUserService current)
    {
        _employeeRepo = employeeRepo;
        _attendanceRepo = attendanceRepo;
        _current = current;
    }

    public async Task<PagedResult<AttendanceRecordDto>> Handle(
    GetAttendanceRecordsQuery request,
    CancellationToken cancellationToken)
{
    if (request.To < request.From)
        throw new InvalidOperationException("Invalid date range.");

    if (string.IsNullOrEmpty(request.EmployeeCode))
        throw new InvalidOperationException("EmployeeCode is required.");

    var pageNumber = request.PageNumber < 1 ? 1 : request.PageNumber;
    var pageSize = request.PageSize < 1 ? 10 : request.PageSize;
    var employeeId = _employeeRepo.GetEmployeeIdByEmployeeCodeAsync(request.EmployeeCode, cancellationToken).Result;

    if (employeeId is null)
        throw new InvalidOperationException("Employee not found.");

    // ADMIN / DIRECTOR
    if (_current.IsInRole(SystemRoles.Admin) ||
        _current.IsInRole(SystemRoles.Director))
    {
        var (records, totalCount) = await _attendanceRepo.GetAttendanceRecordsByEmployeePagedAsync(
            request.From,
            request.To,
            (int)employeeId,
            pageNumber,
            pageSize,
            cancellationToken);

        return new PagedResult<AttendanceRecordDto>(records, totalCount, pageNumber, pageSize);
    }

    // MANAGER
    if (_current.IsInRole(SystemRoles.Manager) && _current.UserId is int mgrId)
    {
        var deptId = await _employeeRepo.GetDepartmentIdByEmployeeIdAsync(mgrId, cancellationToken);

        if (employeeId is int filterId)
        {
            var inDept = await _employeeRepo.IsEmployeeInDepartmentAsync(filterId, (int)deptId, cancellationToken);
            if (!inDept)
                throw new UnauthorizedAccessException();

            var (records, totalCount) = await _attendanceRepo.GetAttendanceRecordsByDepartmentAndEmployeePagedAsync(
                request.From,
                request.To,
                (int)deptId,
                filterId,
                pageNumber,
                pageSize,
                cancellationToken);

            return new PagedResult<AttendanceRecordDto>(records, totalCount, pageNumber, pageSize);
        }
    }

    // EMPLOYEE NORMAL
    if (_current.UserId is int uid)
    {
        var (records, totalCount) = await _attendanceRepo.GetAttendanceRecordsByEmployeePagedAsync(
            request.From,
            request.To,
            uid,
            pageNumber,
            pageSize,
            cancellationToken);

        return new PagedResult<AttendanceRecordDto>(records, totalCount, pageNumber, pageSize);
    }

    throw new UnauthorizedAccessException();
}
}
