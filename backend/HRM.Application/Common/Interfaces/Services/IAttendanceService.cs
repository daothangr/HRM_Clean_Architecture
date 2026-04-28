using HRM.Application.WorkAttendance;

namespace HRM.Application.Common.Interfaces;

public interface IAttendanceService
{
    Task DeleteAttendanceAsync(int attendanceId, CancellationToken cancellationToken);
    Task<int> UpsertAttendanceAsync(UpsertAttendanceCommand request, CancellationToken cancellationToken);
}
