using AutoMapper;
using HRM.Application.Common;
using HRM.Application.Common.Constants;
using HRM.Application.Common.Interfaces;
using MediatR;

namespace HRM.Application.Employees;

public record GetEmployeesQuery(int PageNumber = 1, int PageSize = 10) : IRequest<PagedResult<EmployeeDto>>;

public class GetEmployeesQueryHandler : IRequestHandler<GetEmployeesQuery, PagedResult<EmployeeDto>>
{
    private readonly IEmployeeRepository _employeeRepository;
    private readonly ICurrentUserService _current;
    private readonly IMapper _mapper;

    public GetEmployeesQueryHandler(
        IEmployeeRepository employeeRepository,
        ICurrentUserService current,
        IMapper mapper)
    {
        _employeeRepository = employeeRepository;
        _current = current;
        _mapper = mapper;
    }

    public async Task<PagedResult<EmployeeDto>> Handle(GetEmployeesQuery request, CancellationToken cancellationToken)
    {
        var pageNumber = request.PageNumber < 1 ? 1 : request.PageNumber;
        var pageSize = request.PageSize < 1 ? 10 : Math.Min(request.PageSize, 100);

        // Nếu là admin hoặc director, trả về tất cả nhân viên
        if (_current.IsInRole(SystemRoles.Admin) || _current.IsInRole(SystemRoles.Director))
        {
            var (employees, totalCount) = await _employeeRepository
                .GetEmployeesWithDepartmentPagedAsync(pageNumber, pageSize, cancellationToken);

            return new PagedResult<EmployeeDto>(
                _mapper.Map<List<EmployeeDto>>(employees),
                totalCount,
                pageNumber,
                pageSize);
        }

        // Nếu là manager, chỉ trả về nhân viên trong phòng ban của mình
        if (_current.IsInRole(SystemRoles.Manager) && _current.UserId is int mgrId)
        {
            // Lấy Id phòng ban mà manager đang quản lý
            var deptId = await _employeeRepository.GetDepartmentIdByEmployeeIdAsync(mgrId, cancellationToken);
            if (!deptId.HasValue)
                return new PagedResult<EmployeeDto>(new List<EmployeeDto>(), 0, pageNumber, pageSize);

            // Nếu có Id phòng ban, lấy tất cả nhân viên trong phòng ban đó
            var (employees, totalCount) = await _employeeRepository
                .GetEmployeesByDepartmentPagedAsync(deptId.Value, pageNumber, pageSize, cancellationToken);

            return new PagedResult<EmployeeDto>(
                _mapper.Map<List<EmployeeDto>>(employees),
                totalCount,
                pageNumber,
                pageSize);
        }

        return new PagedResult<EmployeeDto>(new List<EmployeeDto>(), 0, pageNumber, pageSize);
    }
}
