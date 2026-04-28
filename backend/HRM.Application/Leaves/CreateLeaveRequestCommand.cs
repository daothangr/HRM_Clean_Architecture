using FluentValidation;
using HRM.Application.Common.Interfaces;
using HRM.Domain.Enums;
using MediatR;

namespace HRM.Application.Leaves;

public record CreateLeaveRequestCommand(
    LeaveType LeaveType,
    DateTime StartDate,
    DateTime EndDate,
    string? Reason,
    bool IsFullDay = true,
    TimeOnly? StartTime = null,
    TimeOnly? EndTime = null) : IRequest<int>;

public class CreateLeaveRequestCommandHandler : IRequestHandler<CreateLeaveRequestCommand, int>
{
    private readonly ILeaveRequestService _leaveRequestService;
    private readonly ICurrentUserService _current;

    public CreateLeaveRequestCommandHandler(ILeaveRequestService leaveRequestService, ICurrentUserService current)
    {
        _leaveRequestService = leaveRequestService;
        _current = current;
    }

    public async Task<int> Handle(CreateLeaveRequestCommand request, CancellationToken cancellationToken)
    {
        if (_current.UserId is not int userId)
            throw new UnauthorizedAccessException();

        return await _leaveRequestService.CreateLeaveRequestAsync(userId, request, cancellationToken);
    }
}

public class CreateLeaveRequestCommandValidator : AbstractValidator<CreateLeaveRequestCommand>
{
    public CreateLeaveRequestCommandValidator()
    {
        RuleFor(x => x.StartDate).NotEmpty();
        RuleFor(x => x.EndDate).NotEmpty();
        When(x => !x.IsFullDay, () =>
        {
            RuleFor(x => x.StartDate).Equal(x => x.EndDate);
            RuleFor(x => x.StartTime).NotNull();
            RuleFor(x => x.EndTime).NotNull();
        });
    }
}
