using Dapper;
using HRM.Application.Common.Interfaces;
using HRM.Domain.Entities;
using Microsoft.Extensions.Configuration;
using System.Data;

namespace HRM.Infrastructure.Persistence.Repositories;

public class RoleRepository : GenericRepository<Role>, IRoleRepository
{
    private const string SP_GET_ROLE_BY_NAME = "sp_Employees_GetRoleByName";

    public RoleRepository(IConfiguration configuration)
        : base(configuration)
    {
    }

    public async Task<Role?> GetRoleByNameAsync(string roleName, CancellationToken cancellationToken)
    {
        using var conn = CreateConnection();

        return await conn.QueryFirstOrDefaultAsync<Role>(
            new CommandDefinition(
                SP_GET_ROLE_BY_NAME,
                new { RoleName = roleName },
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken
            )
        );
    }
}
