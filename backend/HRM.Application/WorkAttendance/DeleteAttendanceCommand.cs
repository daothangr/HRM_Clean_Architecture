using FluentValidation;
using HRM.Application.Common.Constants;
using HRM.Application.Common.Interfaces;
using MediatR;

namespace HRM.Application.WorkAttendance;

public record DeleteAttendanceCommand(int Id) : IRequest<Unit>;

public class DeleteAttendanceCommandHandler : IRequestHandler<DeleteAttendanceCommand, Unit>
{
    private readonly IAttendanceService _attendanceService;
    private readonly ICurrentUserService _current;

    public DeleteAttendanceCommandHandler(IAttendanceService attendanceService, ICurrentUserService current)
    {
        _attendanceService = attendanceService;
        _current = current;
    }

    public async Task<Unit> Handle(DeleteAttendanceCommand request, CancellationToken cancellationToken)
    {
        if (!_current.IsInRole(SystemRoles.Admin))
            throw new UnauthorizedAccessException();

        await _attendanceService.DeleteAttendanceAsync(request.Id, cancellationToken);
        return Unit.Value;
    }
}

public class DeleteAttendanceCommandValidator : AbstractValidator<DeleteAttendanceCommand>
{
    public DeleteAttendanceCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
    }
}
