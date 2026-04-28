using FluentValidation;
using HRM.Application.Common.Constants;
using HRM.Application.Common.Interfaces;
using MediatR;

namespace HRM.Application.WorkAttendance;

public record UpsertAttendanceCommand(
    int EmployeeId,
    DateTime Date,
    TimeOnly AttendanceTime) : IRequest<int>;

public class UpsertAttendanceCommandHandler : IRequestHandler<UpsertAttendanceCommand, int>
{
    private readonly IAttendanceService _attendanceService;
    private readonly ICurrentUserService _current;

    public UpsertAttendanceCommandHandler(IAttendanceService attendanceService, ICurrentUserService current)
    {
        _attendanceService = attendanceService;
        _current = current;
    }

    public async Task<int> Handle(UpsertAttendanceCommand request, CancellationToken cancellationToken)
    {
        return await _attendanceService.UpsertAttendanceAsync(request, cancellationToken);
    }
}

public class UpsertAttendanceCommandValidator : AbstractValidator<UpsertAttendanceCommand>
{
    public UpsertAttendanceCommandValidator()
    {
        RuleFor(x => x.EmployeeId).GreaterThan(0);
        RuleFor(x => x.Date).NotEmpty();
        RuleFor(x => x.AttendanceTime).NotEmpty();
    }
}
