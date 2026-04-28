using HRM.Application.Common;
using HRM.Application.Common.Constants;
using HRM.Application.Common.Interfaces;
using HRM.Application.Leaves;
using HRM.Domain.Entities;
using HRM.Domain.Enums;
using HRM.Domain.Exceptions;
using HRM.Domain.Interfaces;

namespace HRM.Infrastructure.Services;

public class LeaveRequestService : ILeaveRequestService
{
    private readonly ILeaveRequestRepository _leaveRepo;
    private readonly IEmployeeRepository _employeeRepo;
    private readonly ILeaveBalanceRepository _leaveBalanceRepo;
    private readonly IUnitOfWork _unitOfWork;

    public LeaveRequestService(
        ILeaveRequestRepository leaveRepo,
        IEmployeeRepository employeeRepo,
        ILeaveBalanceRepository leaveBalanceRepo,
        IUnitOfWork unitOfWork)
    {
        _leaveRepo = leaveRepo;
        _employeeRepo = employeeRepo;
        _leaveBalanceRepo = leaveBalanceRepo;
        _unitOfWork = unitOfWork;
    }

    public async Task<int> CreateLeaveRequestAsync(
        int userId,
        CreateLeaveRequestCommand request,
        CancellationToken cancellationToken)
    {
        await _unitOfWork.BeginTransactionAsync();

        try
        {
        if (request.EndDate < request.StartDate)
            throw new InvalidOperationException("End date must be on or after start date.");

        decimal totalDays;
        decimal? totalHours = null;
        TimeOnly? startTime = null;
        TimeOnly? endTime = null;

        if (request.IsFullDay)
        {
            totalDays = CountWorkDays(request.StartDate, request.EndDate);
        }
        else
        {
            if (request.StartDate != request.EndDate)
                throw new InvalidOperationException("Partial-day leave must be on a single calendar day.");
            if (request.StartTime is null || request.EndTime is null)
                throw new InvalidOperationException("Start and end time are required for hourly leave.");

            startTime = request.StartTime.Value;
            endTime = request.EndTime.Value;

            if (endTime <= startTime)
                throw new InvalidOperationException("End time must be after start time.");

            var span = endTime.Value.ToTimeSpan() - startTime.Value.ToTimeSpan();
            var hours = (decimal)span.TotalHours;
            totalHours = hours;
            totalDays = hours / WorkflowConstants.WorkHoursPerDay;

            if (totalDays <= 0)
                throw new InvalidOperationException("Invalid duration.");
        }

        var balance = await _leaveBalanceRepo.GetLeaveBalanceAsync(userId, request.StartDate.Year, cancellationToken);
        if (balance is not null)
        {
            if (request.LeaveType == LeaveType.Annual)
            {
                var available = balance.AnnualLeave - balance.UsedAnnual;
                if (totalDays > available)
                    throw new InvalidOperationException($"Not enough annual leave balance. Available: {available} days.");
            }
            else if (request.LeaveType == LeaveType.Sick)
            {
                var available = balance.SickLeave - balance.UsedSick;
                if (totalDays > available)
                    throw new InvalidOperationException($"Not enough sick leave balance. Available: {available} days.");
            }
        }

        var leaveRequest = new LeaveRequest
        {
            EmployeeId = userId,
            LeaveType = request.LeaveType,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            IsFullDay = request.IsFullDay,
            StartTime = startTime,
            EndTime = endTime,
            TotalHours = totalHours,
            TotalDays = totalDays,
            Reason = request.Reason,
            Status = LeaveRequestStatus.Pending,
            CurrentApprovalLevel = 1,
            CreatedAt = DateTime.UtcNow
        };

        await _leaveRepo.AddAsync(leaveRequest, cancellationToken);
            await _unitOfWork.CommitAsync();

        return leaveRequest.Id;
        }
        catch
        {
            await _unitOfWork.RollbackAsync();
            throw;
        }
    }

    public async Task CancelLeaveRequestAsync(
        int leaveRequestId,
        int requesterId,
        CancellationToken cancellationToken)
    {
        var leaveRequest = await _leaveRepo.GetByIdAsync(leaveRequestId, cancellationToken)
            ?? throw new NotFoundException(nameof(LeaveRequest), leaveRequestId);

        if (leaveRequest.EmployeeId != requesterId)
            throw new UnauthorizedAccessException();

        if (leaveRequest.Status != LeaveRequestStatus.Pending)
            throw new DomainException("Leave request is not pending.");

        leaveRequest.Status = LeaveRequestStatus.Cancelled;
        leaveRequest.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.BeginTransactionAsync();

        try
        {
            await _leaveRepo.UpdateAsync(leaveRequest, cancellationToken);
            await _unitOfWork.CommitAsync();
        }
        catch
        {
            await _unitOfWork.RollbackAsync();
            throw;
        }
    }

    public async Task ProcessLeaveRequestAsync(
        int leaveRequestId,
        bool approve,
        string? comment,
        int approverId,
        CancellationToken cancellationToken)
    {
        var leaveRequest = await _leaveRepo.GetByIdAsync(leaveRequestId, cancellationToken)
            ?? throw new NotFoundException(nameof(LeaveRequest), leaveRequestId);

        if (leaveRequest.Status != LeaveRequestStatus.Pending)
            throw new DomainException("Leave request is not pending.");

        var approver = await _employeeRepo.GetEmployeeByIdWithDepartmentAndRolesAsync(approverId, cancellationToken)
            ?? throw new NotFoundException(nameof(Employee), approverId);

        var requester = await _employeeRepo.GetEmployeeByIdWithDepartmentAndRolesAsync(leaveRequest.EmployeeId, cancellationToken)
            ?? throw new NotFoundException(nameof(Employee), leaveRequest.EmployeeId);

        var approverRoles = approver.UserRoles.Select(x => x.Role.Name).ToHashSet();

        // Kiểm tra quyền của approver dựa trên cấp phê duyệt và số ngày nghỉ
        if (!approve)
        {
            await RejectAsync(leaveRequest, approverId, comment, cancellationToken);
            return;
        }

        if (leaveRequest.CurrentApprovalLevel == 1)
        {
            if (!WorkflowApproval.CanApproveLevel1(approverRoles, approver, requester))
                throw new UnauthorizedAccessException();

            if (leaveRequest.TotalDays <= WorkflowConstants.ManagerOnlyDayThreshold
                || approverRoles.Contains(SystemRoles.Director)
                || approverRoles.Contains(SystemRoles.Admin))
            {
                await ApproveAsync(leaveRequest, approverId, 1, comment, cancellationToken);
                return;
            }

            await EscalateAsync(leaveRequest, approverId, comment, cancellationToken);
            return;
        }

        if (leaveRequest.CurrentApprovalLevel == 2)
        {
            if (!approverRoles.Contains(SystemRoles.Director)
                && !approverRoles.Contains(SystemRoles.Admin))
                throw new UnauthorizedAccessException();

            await ApproveAsync(leaveRequest, approverId, 2, comment, cancellationToken);
            return;
        }

        throw new DomainException("Invalid approval level.");
    }

    private Task RejectAsync(LeaveRequest leaveRequest, int approverId, string? comment, CancellationToken ct)
    {
        leaveRequest.Status = LeaveRequestStatus.Rejected;
        leaveRequest.UpdatedAt = DateTime.UtcNow;

        return SaveAsync(
            leaveRequest,
            BuildApprovalLog(leaveRequest.Id, approverId, leaveRequest.CurrentApprovalLevel, ApprovalAction.Rejected, comment),
            ct);
    }

    private Task ApproveAsync(LeaveRequest leaveRequest, int approverId, int level, string? comment, CancellationToken ct)
    {
        leaveRequest.Status = LeaveRequestStatus.Approved;
        leaveRequest.UpdatedAt = DateTime.UtcNow;

        return SaveAndUpdateBalanceAsync(
            leaveRequest,
            BuildApprovalLog(leaveRequest.Id, approverId, level, ApprovalAction.Approved, comment),
            ct);
    }

    private Task EscalateAsync(LeaveRequest leaveRequest, int approverId, string? comment, CancellationToken ct)
    {
        leaveRequest.CurrentApprovalLevel = 2;
        leaveRequest.UpdatedAt = DateTime.UtcNow;

        return SaveAsync(
            leaveRequest,
            BuildApprovalLog(leaveRequest.Id, approverId, 1, ApprovalAction.Approved, comment),
            ct);
    }

    private async Task SaveAsync(LeaveRequest leaveRequest, ApprovalLog approvalLog, CancellationToken ct)
    {
        await _unitOfWork.BeginTransactionAsync();

        try
        {
            await _leaveRepo.UpdateAsync(leaveRequest, ct);
            await _leaveRepo.AddApprovalLogAsync(approvalLog, ct);
            await _unitOfWork.CommitAsync();
        }
        catch
        {
            await _unitOfWork.RollbackAsync();
            throw;
        }
    }

    private async Task SaveAndUpdateBalanceAsync(LeaveRequest leaveRequest, ApprovalLog approvalLog, CancellationToken ct)
    {
        await _unitOfWork.BeginTransactionAsync();

        try
        {
            await _leaveRepo.UpdateAsync(leaveRequest, ct);
            await _leaveRepo.AddApprovalLogAsync(approvalLog, ct);

            // Update LeaveBalance
            var year = leaveRequest.StartDate.Year;
            var leaveBalance = await _leaveBalanceRepo.GetOrCreateLeaveBalanceAsync(
                leaveRequest.EmployeeId, year, ct);

            if (leaveBalance is not null)
            {
                if (leaveRequest.LeaveType == LeaveType.Annual)
                {
                    leaveBalance.UsedAnnual += leaveRequest.TotalDays;
                }
                else if (leaveRequest.LeaveType == LeaveType.Sick)
                {
                    leaveBalance.UsedSick += leaveRequest.TotalDays;
                }

                await _leaveBalanceRepo.UpdateLeaveBalanceAsync(leaveBalance, ct);
            }

            await _unitOfWork.CommitAsync();
        }
        catch
        {
            await _unitOfWork.RollbackAsync();
            throw;
        }
    }

    private static ApprovalLog BuildApprovalLog(
        int requestId,
        int approverId,
        int level,
        ApprovalAction action,
        string? comment)
    {
        return new ApprovalLog
        {
            RequestId = requestId,
            RequestType = "leave",
            ApproverId = approverId,
            Action = action,
            Comment = comment,
            Level = level,
            CreatedAt = DateTime.UtcNow
        };
    }

    private static decimal CountWorkDays(DateTime start, DateTime end)
    {
        var days = 0m;
        for (var d = start.Date; d <= end.Date; d = d.AddDays(1))
        {
            if (d.DayOfWeek is not DayOfWeek.Saturday and not DayOfWeek.Sunday)
                days += 1;
        }

        return days < 1 ? 1 : days;
    }
}
