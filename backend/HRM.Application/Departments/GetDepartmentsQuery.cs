using AutoMapper;
using HRM.Application.Common.Constants;
using HRM.Application.Common.Interfaces;
using MediatR;

namespace HRM.Application.Departments;

public record GetDepartmentsQuery(bool IncludeInactive = false) : IRequest<List<DepartmentDto>>;

public class GetDepartmentsQueryHandler : IRequestHandler<GetDepartmentsQuery, List<DepartmentDto>>
{
    private readonly IDepartmentRepository _departmentRepository;
    private readonly IMapper _mapper;
    private readonly ICurrentUserService _current;

    public GetDepartmentsQueryHandler(
        IDepartmentRepository departmentRepository,
        IMapper mapper,
        ICurrentUserService current)
    {
        _departmentRepository = departmentRepository;
        _mapper = mapper;
        _current = current;
    }

    public async Task<List<DepartmentDto>> Handle(GetDepartmentsQuery request, CancellationToken cancellationToken)
    {
        var includeInactive = request.IncludeInactive && _current.IsInRole(SystemRoles.Admin);

        var departments = await _departmentRepository.GetDepartmentsAsync(includeInactive, cancellationToken);
        return _mapper.Map<List<DepartmentDto>>(departments);
    }
}
