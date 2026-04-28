using AutoMapper;
using HRM.Application.Common.Constants;
using HRM.Application.Common.Interfaces;
using MediatR;

namespace HRM.Application.Employees;

public record GetManagersQuery : IRequest<List<EmployeeListDto>>;

public class GetManagersQueryHandler : IRequestHandler<GetManagersQuery, List<EmployeeListDto>>
{
    private readonly IEmployeeRepository _employeeRepository;
    private readonly IMapper _mapper;

    public GetManagersQueryHandler(IEmployeeRepository employeeRepository, IMapper mapper)
    {
        _employeeRepository = employeeRepository;
        _mapper = mapper;
    }

    public async Task<List<EmployeeListDto>> Handle(GetManagersQuery request, CancellationToken cancellationToken)
    {
        var managers = await _employeeRepository.GetEmployeesByRoleNameAsync(SystemRoles.Manager, cancellationToken);
        return _mapper.Map<List<EmployeeListDto>>(managers);
    }
}
