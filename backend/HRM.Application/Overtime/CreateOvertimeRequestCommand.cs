using FluentValidation;
using HRM.Application.Common.Interfaces;
using HRM.Domain.Entities;
using HRM.Domain.Enums;
using MediatR;

namespace HRM.Application.Overtime;

public record CreateOvertimeRequestCommand(
    DateTime WorkDate,
    TimeOnly StartTime,
    TimeOnly EndTime,
    string? Reason) : IRequest<int>;

public class CreateOvertimeRequestCommandHandler : IRequestHandler<CreateOvertimeRequestCommand, int>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _current;

    public CreateOvertimeRequestCommandHandler(IApplicationDbContext db, ICurrentUserService current)
    {
        _db = db;
        _current = current;
    }

    public async Task<int> Handle(CreateOvertimeRequestCommand request, CancellationToken cancellationToken)
    {
        if (_current.UserId is not int userId)
            throw new UnauthorizedAccessException();

        if (request.EndTime <= request.StartTime)
            throw new InvalidOperationException("End time must be after start time.");

        var span = request.EndTime - request.StartTime;
        var hours = (decimal)span.TotalHours;
        if (hours <= 0)
            throw new InvalidOperationException("Invalid OT duration.");

        var ot = new OvertimeRequest
        {
            EmployeeId = userId,
            WorkDate = request.WorkDate,
            StartTime = request.StartTime,
            EndTime = request.EndTime,
            TotalHours = hours,
            Reason = request.Reason,
            Status = LeaveRequestStatus.Pending,
            CurrentApprovalLevel = 1,
            CreatedAt = DateTime.UtcNow
        };

        _db.OvertimeRequests.Add(ot);
        await _db.SaveChangesAsync(cancellationToken);
        return ot.Id;
    }
}

public class CreateOvertimeRequestCommandValidator : AbstractValidator<CreateOvertimeRequestCommand>
{
    public CreateOvertimeRequestCommandValidator()
    {
        RuleFor(x => x.WorkDate).NotEmpty();
    }
}
