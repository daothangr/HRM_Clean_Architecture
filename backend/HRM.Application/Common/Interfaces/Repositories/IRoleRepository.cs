using HRM.Application.Common.Interfaces.Repositories;
using HRM.Domain.Entities;

namespace HRM.Application.Common.Interfaces;

public interface IRoleRepository : IGenericRepository<Role>
{
    Task<Role?> GetRoleByNameAsync(string roleName, CancellationToken cancellationToken);
}
