using AutoMapper;
using HRM.Application.Common.Interfaces;
using MediatR;

namespace HRM.Application.Roles;

public record GetRolesQuery : IRequest<List<RoleDto>>;

public class GetRolesQueryHandler : IRequestHandler<GetRolesQuery, List<RoleDto>>
{
    private readonly IRoleRepository _roleRepository;
    private readonly IMapper _mapper;

    public GetRolesQueryHandler(
        IRoleRepository roleRepository,
        IMapper mapper)
    {
        _roleRepository = roleRepository;
        _mapper = mapper;
    }

    public async Task<List<RoleDto>> Handle(GetRolesQuery request, CancellationToken cancellationToken)
    {
        var roles = await _roleRepository.ListAsync(cancellationToken);
        return _mapper.Map<List<RoleDto>>(roles);
    }
}

public class RoleDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
}
