using AutoMapper;
using HRM.Application.Common.Constants;
using HRM.Application.Common.Interfaces;
using HRM.Domain.Exceptions;
using MediatR;

namespace HRM.Application.Employees;

public record GetEmployeeByIdQuery(int Id) : IRequest<EmployeeDto>;

public class GetEmployeeByIdQueryHandler : IRequestHandler<GetEmployeeByIdQuery, EmployeeDto>
{
    private readonly IEmployeeRepository _employeeRepository;
    private readonly ICurrentUserService _current;
    private readonly IMapper _mapper;

    public GetEmployeeByIdQueryHandler(
        IEmployeeRepository employeeRepository,
        ICurrentUserService current,
        IMapper mapper)
    {
        _employeeRepository = employeeRepository;
        _current = current;
        _mapper = mapper;
    }

    public async Task<EmployeeDto> Handle(GetEmployeeByIdQuery request, CancellationToken cancellationToken)
    {
        var entity = await _employeeRepository.GetEmployeeByIdWithDepartmentAndRolesAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Domain.Entities.Employee), request.Id);

        if (_current.IsInRole(SystemRoles.Admin) || _current.IsInRole(SystemRoles.Director))
        {
            return Map(entity);
        }

        if (_current.IsInRole(SystemRoles.Manager) && _current.UserId is int mgrId)
        {
            var mgrDept = await _employeeRepository.GetDepartmentIdByEmployeeIdAsync(mgrId, cancellationToken);
            if (!mgrDept.HasValue)
                throw new UnauthorizedAccessException();
            if (entity.DepartmentId != mgrDept.Value)
                throw new UnauthorizedAccessException();
            return Map(entity);
        }

        if (_current.UserId == request.Id)
            return Map(entity);

        throw new UnauthorizedAccessException();
    }

    private EmployeeDto Map(Domain.Entities.Employee entity) => _mapper.Map<EmployeeDto>(entity);
}
