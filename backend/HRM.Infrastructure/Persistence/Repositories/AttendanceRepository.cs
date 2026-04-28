using Dapper;
using HRM.Application.Common.Interfaces.Repositories;
using HRM.Application.WorkAttendance;
using HRM.Domain.Entities;
using Microsoft.Extensions.Configuration;
using System.Data;

namespace HRM.Infrastructure.Persistence.Repositories
{
    public class AttendanceRepository : GenericRepository<Attendance>, IAttendanceRepository
    {
        private const string SpGetAttendanceRecordsByEmployeePaged = "sp_Attendance_GetAttendanceRecordsByEmployeePaged";
        private const string SpGetAttendanceByDeptAndEmployeePaged = "sp_Attendance_GetAttendanceByDeptAndEmployeePaged";
        private const string SpCheckExistsByEmployeeAndDate = "sp_Attendance_CheckExistsByEmployeeAndDate";
        private const string SpGetByEmployeeAndDate = "sp_Attendance_GetByEmployeeAndDate";

        public AttendanceRepository(IConfiguration configuration)
        : base(configuration)
        {
        }

        public async Task<bool> ExistsByEmployeeAndDateAsync(int employeeId, DateTime date, CancellationToken cancellationToken)
        {
            using var conn = CreateConnection();

            var command = new CommandDefinition(
                SpCheckExistsByEmployeeAndDate,
                new { EmployeeId = employeeId, AttendanceDate = date.Date },
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken);

            var result = await conn.ExecuteScalarAsync<int>(command);
            return result > 0;
        }

        public async Task<Attendance?> GetByEmployeeAndDateAsync(int employeeId, DateTime date, CancellationToken cancellationToken)
        {
            using var conn = CreateConnection();

            var command = new CommandDefinition(
                SpGetByEmployeeAndDate,
                new { EmployeeId = employeeId, AttendanceDate = date.Date },
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken);

            return await conn.QueryFirstOrDefaultAsync<Attendance>(command);
        }

        public async Task<(List<AttendanceRecordDto> Records, int TotalCount)> GetAttendanceRecordsByDepartmentAndEmployeePagedAsync(DateTime from, DateTime to, int deptId, int employeeId, int pageNumber, int pageSize, CancellationToken cancellationToken)
        {
            using var conn = CreateConnection();

            using var multi = await conn.QueryMultipleAsync(
                new CommandDefinition(
                SpGetAttendanceByDeptAndEmployeePaged,
                new { From = from, To = to, DeptId = deptId, EmployeeId = employeeId, PageNumber = pageNumber, PageSize = pageSize },
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken));

            var records = (await multi.ReadAsync<AttendanceRecordDto>()).ToList();
            var totalCount = await multi.ReadFirstOrDefaultAsync<int>();
            return (records, totalCount);
        }

        public async Task<(List<AttendanceRecordDto> Records, int TotalCount)> GetAttendanceRecordsByEmployeePagedAsync(DateTime from, DateTime to, int employeeId, int pageNumber, int pageSize, CancellationToken cancellationToken)
        {
            using var conn = CreateConnection();

            using var multi = await conn.QueryMultipleAsync(
                new CommandDefinition(
                SpGetAttendanceRecordsByEmployeePaged,
                new { From = from, To = to, EmployeeId = employeeId, PageNumber = pageNumber, PageSize = pageSize },
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken));

            var records = (await multi.ReadAsync<AttendanceRecordDto>()).ToList();
            var totalCount = await multi.ReadFirstOrDefaultAsync<int>();
            return (records, totalCount);
        }
    }
}
