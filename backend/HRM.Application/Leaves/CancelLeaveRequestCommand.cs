using FluentValidation;
using HRM.Application.Common.Interfaces;
using MediatR;

namespace HRM.Application.Leaves;

public record CancelLeaveRequestCommand(int LeaveRequestId) : IRequest<Unit>;

public class CancelLeaveRequestCommandHandler : IRequestHandler<CancelLeaveRequestCommand, Unit>
{
    private readonly ILeaveRequestService _leaveRequestService;
    private readonly ICurrentUserService _current;

    public CancelLeaveRequestCommandHandler(
        ILeaveRequestService leaveRequestService,
        ICurrentUserService current)
    {
        _leaveRequestService = leaveRequestService;
        _current = current;
    }

    public async Task<Unit> Handle(CancelLeaveRequestCommand request, CancellationToken cancellationToken)
    {
        if (_current.UserId is not int requesterId)
            throw new UnauthorizedAccessException();

        await _leaveRequestService.CancelLeaveRequestAsync(
            request.LeaveRequestId,
            requesterId,
            cancellationToken);

        return Unit.Value;
    }
}

public class CancelLeaveRequestCommandValidator : AbstractValidator<CancelLeaveRequestCommand>
{
    public CancelLeaveRequestCommandValidator()
    {
        RuleFor(x => x.LeaveRequestId).GreaterThan(0);
    }
}