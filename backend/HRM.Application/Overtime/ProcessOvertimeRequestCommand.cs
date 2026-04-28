using FluentValidation;
using HRM.Application.Common;
using HRM.Application.Common.Constants;
using HRM.Application.Common.Interfaces;
using HRM.Domain.Entities;
using HRM.Domain.Enums;
using HRM.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HRM.Application.Overtime;

public record ProcessOvertimeRequestCommand(int OvertimeRequestId, bool Approve, string? Comment)
    : IRequest<Unit>;

public class ProcessOvertimeRequestCommandHandler : IRequestHandler<ProcessOvertimeRequestCommand, Unit>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _current;

    public ProcessOvertimeRequestCommandHandler(IApplicationDbContext db, ICurrentUserService current)
    {
        _db = db;
        _current = current;
    }

    public async Task<Unit> Handle(ProcessOvertimeRequestCommand request, CancellationToken cancellationToken)
    {
        if (_current.UserId is not int approverId)
            throw new UnauthorizedAccessException();

        var ot = await _db.OvertimeRequests
            .Include(x => x.Employee)
            .ThenInclude(e => e.Department)
            .FirstOrDefaultAsync(x => x.Id == request.OvertimeRequestId, cancellationToken)
            ?? throw new NotFoundException(nameof(OvertimeRequest), request.OvertimeRequestId);

        if (ot.Status != LeaveRequestStatus.Pending)
            throw new DomainException("Overtime request is not pending.");

        var approver = await _db.Employees
            .Include(e => e.UserRoles)
            .ThenInclude(ur => ur.Role)
            .FirstAsync(e => e.Id == approverId, cancellationToken);

        var approverRoles = approver.UserRoles.Select(ur => ur.Role.Name).ToHashSet();
        var requester = ot.Employee;
        var dayEquivalent = ot.TotalHours / WorkflowConstants.WorkHoursPerDay;

        if (!request.Approve)
        {
            ot.Status = LeaveRequestStatus.Rejected;
            ot.UpdatedAt = DateTime.UtcNow;
            AddLog(ot.Id, approverId, ot.CurrentApprovalLevel, ApprovalAction.Rejected, request.Comment);
            await _db.SaveChangesAsync(cancellationToken);
            return Unit.Value;
        }

        if (ot.CurrentApprovalLevel == 1)
        {
            if (!WorkflowApproval.CanApproveLevel1(approverRoles, approver, requester))
                throw new UnauthorizedAccessException("You cannot approve this request at this level.");

            if (dayEquivalent <= WorkflowConstants.ManagerOnlyDayThreshold)
            {
                ot.Status = LeaveRequestStatus.Approved;
                ot.UpdatedAt = DateTime.UtcNow;
                AddLog(ot.Id, approverId, ot.CurrentApprovalLevel, ApprovalAction.Approved, request.Comment);
                await ApplyOvertimeToAttendanceAsync(ot, cancellationToken);
                await _db.SaveChangesAsync(cancellationToken);
                return Unit.Value;
            }

            if (approverRoles.Contains(SystemRoles.Director) || approverRoles.Contains(SystemRoles.Admin))
            {
                ot.Status = LeaveRequestStatus.Approved;
                ot.UpdatedAt = DateTime.UtcNow;
                AddLog(ot.Id, approverId, ot.CurrentApprovalLevel, ApprovalAction.Approved, request.Comment);
                await ApplyOvertimeToAttendanceAsync(ot, cancellationToken);
                await _db.SaveChangesAsync(cancellationToken);
                return Unit.Value;
            }

            ot.CurrentApprovalLevel = 2;
            ot.UpdatedAt = DateTime.UtcNow;
            AddLog(ot.Id, approverId, 1, ApprovalAction.Approved, request.Comment);
            await _db.SaveChangesAsync(cancellationToken);
            return Unit.Value;
        }

        if (ot.CurrentApprovalLevel == 2)
        {
            if (!approverRoles.Contains(SystemRoles.Director) && !approverRoles.Contains(SystemRoles.Admin))
                throw new UnauthorizedAccessException("Director or Admin approval required.");

            ot.Status = LeaveRequestStatus.Approved;
            ot.UpdatedAt = DateTime.UtcNow;
            AddLog(ot.Id, approverId, 2, ApprovalAction.Approved, request.Comment);
            await ApplyOvertimeToAttendanceAsync(ot, cancellationToken);
            await _db.SaveChangesAsync(cancellationToken);
            return Unit.Value;
        }

        throw new DomainException("Invalid approval level.");
    }

    private async Task ApplyOvertimeToAttendanceAsync(OvertimeRequest ot, CancellationToken cancellationToken)
    {
        var att = await _db.Attendances
            .FirstOrDefaultAsync(
                a => a.EmployeeId == ot.EmployeeId && a.Date == ot.WorkDate,
                cancellationToken);
        if (att == null)
        {
            att = new Attendance
            {
                EmployeeId = ot.EmployeeId,
                Date = ot.WorkDate,
                Status = 1
            };
            _db.Attendances.Add(att);
        }

        att.OvertimeHours += ot.TotalHours;
    }

    private void AddLog(int requestId, int approverId, int level, ApprovalAction action, string? comment)
    {
        _db.ApprovalLogs.Add(new ApprovalLog
        {
            RequestId = requestId,
            RequestType = "overtime",
            ApproverId = approverId,
            Action = action,
            Comment = comment,
            Level = level,
            CreatedAt = DateTime.UtcNow
        });
    }
}

public class ProcessOvertimeRequestCommandValidator : AbstractValidator<ProcessOvertimeRequestCommand>
{
    public ProcessOvertimeRequestCommandValidator()
    {
        RuleFor(x => x.OvertimeRequestId).GreaterThan(0);
    }
}
