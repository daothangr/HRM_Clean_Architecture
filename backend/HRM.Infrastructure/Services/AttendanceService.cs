using HRM.Application.Common.Interfaces;
using HRM.Application.Common.Interfaces.Repositories;
using HRM.Application.Common.Constants;
using HRM.Application.WorkAttendance;
using HRM.Domain.Entities;
using HRM.Domain.Enums;
using HRM.Domain.Exceptions;
using HRM.Domain.Interfaces;

namespace HRM.Infrastructure.Services;

public class AttendanceService : IAttendanceService
{
    private readonly IEmployeeRepository _employeeRepository;
    private readonly IAttendanceRepository _attendanceRepository;
    private readonly IUnitOfWork _unitOfWork;

    public AttendanceService(
        IEmployeeRepository employeeRepository,
        IAttendanceRepository attendanceRepository,
        IUnitOfWork unitOfWork)
    {
        _employeeRepository = employeeRepository;
        _attendanceRepository = attendanceRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<int> UpsertAttendanceAsync(UpsertAttendanceCommand request, CancellationToken cancellationToken)
    {
        var employee = await _employeeRepository.GetByIdAsync(request.EmployeeId, cancellationToken)
            ?? throw new InvalidOperationException("Employee not found.");

        if (!employee.IsActive)
            throw new InvalidOperationException("Employee account is inactive.");

        await _unitOfWork.BeginTransactionAsync();

        try
        {
            var attendanceDate = request.Date.Date;

            var entity = await _attendanceRepository.GetByEmployeeAndDateAsync(
                request.EmployeeId,
                attendanceDate,
                cancellationToken);

            if (entity is null)
            {
                entity = new Attendance
                {
                    EmployeeId = request.EmployeeId,
                    Date = attendanceDate,
                    CheckInTime = request.AttendanceTime,
                    CheckOutTime = null,
                    Status = (byte)ResolveAttendanceStatus(request.AttendanceTime, null),
                    OvertimeHours = 0,
                    WorkHours = null,
                };

                await _attendanceRepository.AddAsync(entity, cancellationToken);
            }
            else
            {
                if (!entity.CheckInTime.HasValue)
                    throw new InvalidOperationException("Check-in time is missing for this attendance record.");

                if (request.AttendanceTime < entity.CheckInTime.Value)
                    throw new InvalidOperationException("Check-out time cannot be earlier than check-in time.");

                entity.CheckOutTime = request.AttendanceTime;

                var workSpan = entity.CheckOutTime.Value - entity.CheckInTime.Value;
                entity.WorkHours = (decimal)workSpan.TotalHours;
                entity.Status = (byte)ResolveAttendanceStatus(entity.CheckInTime.Value, entity.CheckOutTime);
                entity.OvertimeHours = CalculateOvertimeHours(entity.CheckOutTime);

                await _attendanceRepository.UpdateAsync(entity, cancellationToken);
            }

            await _unitOfWork.CommitAsync();
            return entity.Id;
        }
        catch
        {
            await _unitOfWork.RollbackAsync();
            throw;
        }
    }

    public async Task DeleteAttendanceAsync(int attendanceId, CancellationToken cancellationToken)
    {
        await _unitOfWork.BeginTransactionAsync();

        try
        {
            var attendance = await _attendanceRepository.GetByIdAsync(attendanceId, cancellationToken)
                ?? throw new NotFoundException(nameof(Attendance), attendanceId);

            await _attendanceRepository.RemoveAsync(attendance, cancellationToken);
            await _unitOfWork.CommitAsync();
        }
        catch
        {
            await _unitOfWork.RollbackAsync();
            throw;
        }
    }

    private static AttendanceStatus ResolveAttendanceStatus(TimeOnly checkInTime, TimeOnly? checkOutTime)
    {
        if (checkInTime > OfficeHours.OfficeStartTime)
            return AttendanceStatus.Late;

        if (checkOutTime.HasValue && checkOutTime.Value < OfficeHours.OfficeEndTime)
            return AttendanceStatus.EarlyLeave;

        return AttendanceStatus.Present;
    }

    private static decimal CalculateOvertimeHours(TimeOnly? checkOutTime)
    {
        if (!checkOutTime.HasValue || checkOutTime.Value <= OfficeHours.OfficeEndTime)
            return 0;

        return (decimal)(checkOutTime.Value - OfficeHours.OfficeEndTime).TotalHours;
    }
}
