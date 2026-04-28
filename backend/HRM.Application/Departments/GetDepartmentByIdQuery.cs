using AutoMapper;
using HRM.Application.Common.Interfaces;
using HRM.Domain.Exceptions;
using MediatR;

namespace HRM.Application.Departments;

public record GetDepartmentByIdQuery(int Id) : IRequest<DepartmentDto>;

public class GetDepartmentByIdQueryHandler : IRequestHandler<GetDepartmentByIdQuery, DepartmentDto>
{
    private readonly IDepartmentRepository _departmentRepository;
    private readonly IMapper _mapper;

    public GetDepartmentByIdQueryHandler(
        IDepartmentRepository departmentRepository,
        IMapper mapper)
    {
        _departmentRepository = departmentRepository;
        _mapper = mapper;
    }

    public async Task<DepartmentDto> Handle(GetDepartmentByIdQuery request, CancellationToken cancellationToken)
    {
        var department = await _departmentRepository.GetDepartmentByIdAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Domain.Entities.Department), request.Id);

        return _mapper.Map<DepartmentDto>(department);
    }
}