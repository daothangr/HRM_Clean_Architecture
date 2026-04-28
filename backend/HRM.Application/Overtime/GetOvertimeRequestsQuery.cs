using AutoMapper;
using AutoMapper.QueryableExtensions;
using HRM.Application.Common.Constants;
using HRM.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HRM.Application.Overtime;

public record GetOvertimeRequestsQuery : IRequest<List<OvertimeRequestDto>>;

public class GetOvertimeRequestsQueryHandler : IRequestHandler<GetOvertimeRequestsQuery, List<OvertimeRequestDto>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _current;
    private readonly IMapper _mapper;

    public GetOvertimeRequestsQueryHandler(
        IApplicationDbContext db,
        ICurrentUserService current,
        IMapper mapper)
    {
        _db = db;
        _current = current;
        _mapper = mapper;
    }

    public async Task<List<OvertimeRequestDto>> Handle(GetOvertimeRequestsQuery request, CancellationToken cancellationToken)
    {
        var query = _db.OvertimeRequests
            .AsNoTracking()
            .Include(x => x.Employee)
            .ThenInclude(e => e.Department);

        if (_current.IsInRole(SystemRoles.Admin) || _current.IsInRole(SystemRoles.Director))
        {
            return await query
                .OrderByDescending(x => x.CreatedAt)
                .ProjectTo<OvertimeRequestDto>(_mapper.ConfigurationProvider)
                .ToListAsync(cancellationToken);
        }

        if (_current.IsInRole(SystemRoles.Manager) && _current.UserId is int mgrId)
        {
            var deptId = await _db.Employees.Where(e => e.Id == mgrId).Select(e => e.DepartmentId)
                .FirstAsync(cancellationToken);
            return await query
                .Where(x => x.Employee.DepartmentId == deptId || x.EmployeeId == mgrId)
                .OrderByDescending(x => x.CreatedAt)
                .ProjectTo<OvertimeRequestDto>(_mapper.ConfigurationProvider)
                .ToListAsync(cancellationToken);
        }

        if (_current.UserId is int uid)
        {
            return await query
                .Where(x => x.EmployeeId == uid)
                .OrderByDescending(x => x.CreatedAt)
                .ProjectTo<OvertimeRequestDto>(_mapper.ConfigurationProvider)
                .ToListAsync(cancellationToken);
        }

        return new List<OvertimeRequestDto>();
    }
}
