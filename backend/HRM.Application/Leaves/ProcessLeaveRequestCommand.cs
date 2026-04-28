using FluentValidation;
using HRM.Application.Common.Interfaces;
using MediatR;

namespace HRM.Application.Leaves;

public record ProcessLeaveRequestCommand(int LeaveRequestId, bool Approve, string? Comment)
    : IRequest<Unit>;

public class ProcessLeaveRequestCommandHandler 
    : IRequestHandler<ProcessLeaveRequestCommand, Unit>
{
    private readonly ILeaveRequestService _leaveRequestService;
    private readonly ICurrentUserService _current;

    public ProcessLeaveRequestCommandHandler(
        ILeaveRequestService leaveRequestService,
        ICurrentUserService current)
    {
        _leaveRequestService = leaveRequestService;
        _current = current;
    }

    public async Task<Unit> Handle(ProcessLeaveRequestCommand request, CancellationToken cancellationToken)
    {
        if (_current.UserId is not int approverId)
            throw new UnauthorizedAccessException();

        await _leaveRequestService.ProcessLeaveRequestAsync(
            request.LeaveRequestId,
            request.Approve,
            request.Comment,
            approverId,
            cancellationToken);

        return Unit.Value;
    }
}

public class ProcessLeaveRequestCommandValidator : AbstractValidator<ProcessLeaveRequestCommand>
{
    public ProcessLeaveRequestCommandValidator()
    {
        RuleFor(x => x.LeaveRequestId).GreaterThan(0);
    }
}
