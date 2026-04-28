using System;
using System.Collections.Generic;
using System.Text;
using HRM.Application.WorkAttendance;
using HRM.Domain.Entities;

namespace HRM.Application.Common.Interfaces.Repositories
{
    public interface IAttendanceRepository : IGenericRepository<Attendance>
    {
        Task<(List<AttendanceRecordDto> Records, int TotalCount)> GetAttendanceRecordsByEmployeePagedAsync(DateTime from, DateTime to, int employeeId, int pageNumber, int pageSize, CancellationToken cancellationToken);

        Task<(List<AttendanceRecordDto> Records, int TotalCount)> GetAttendanceRecordsByDepartmentAndEmployeePagedAsync(DateTime from, DateTime to, int deptId, int employeeId, int pageNumber, int pageSize, CancellationToken cancellationToken);
        Task<Attendance?> GetByEmployeeAndDateAsync(int employeeId, DateTime date, CancellationToken cancellationToken);
        // Kiểm tra trùng lặp khi tạo mới hoặc cập nhật (không tính bản ghi hiện tại khi cập nhật)
        Task<bool> ExistsByEmployeeAndDateAsync(int employeeId, DateTime date, CancellationToken cancellationToken);
    }
}
