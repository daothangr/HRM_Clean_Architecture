using Dapper;
using HRM.Application.Common.Interfaces;
using HRM.Domain.Entities;
using Microsoft.Extensions.Configuration;
using System.Data;

namespace HRM.Infrastructure.Persistence.Repositories;

public class LeaveBalanceRepository : GenericRepository<LeaveBalance>, ILeaveBalanceRepository
{
    private const string SP_GET_LEAVE_BALANCE = "sp_LeaveBalances_GetByEmployeeAndYear";

    public LeaveBalanceRepository(IConfiguration configuration)
        : base(configuration)
    {
    }

    public async Task<LeaveBalance?> GetOrCreateLeaveBalanceAsync(int employeeId, int year, CancellationToken cancellationToken)
    {
        var balance = await GetLeaveBalanceAsync(employeeId, year, cancellationToken);

        if (balance is not null)
            return balance;

        // Create new leave balance with default values
        var newBalance = new LeaveBalance
        {
            EmployeeId = employeeId,
            Year = year,
            AnnualLeave = 12,
            SickLeave = 3,
            UsedAnnual = 0,
            UsedSick = 0
        };

        await AddAsync(newBalance, cancellationToken);
        return newBalance;
    }

    public async Task<LeaveBalance?> GetLeaveBalanceAsync(int employeeId, int year, CancellationToken cancellationToken)
    {
        using var conn = CreateConnection();

        var command = new CommandDefinition(
            SP_GET_LEAVE_BALANCE,
            new { EmployeeId = employeeId, Year = year },
            commandType: CommandType.StoredProcedure,
            cancellationToken: cancellationToken);

        return await conn.QueryFirstOrDefaultAsync<LeaveBalance>(command);
    }

    public async Task UpdateLeaveBalanceAsync(LeaveBalance leaveBalance, CancellationToken cancellationToken)
    {
        await UpdateAsync(leaveBalance, cancellationToken);
    }

    protected new IDbConnection CreateConnection()
    {
        return base.CreateConnection();
    }
}
