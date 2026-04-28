using HRM.Application.Leaves;

namespace HRM.Application.Common.Interfaces;

public interface ILeaveRequestService
{
    Task<int> CreateLeaveRequestAsync(
        int userId,
        CreateLeaveRequestCommand request,
        CancellationToken cancellationToken);

    Task CancelLeaveRequestAsync(
        int leaveRequestId,
        int requesterId,
        CancellationToken cancellationToken);

    Task ProcessLeaveRequestAsync(
        int leaveRequestId,
        bool approve,
        string? comment,
        int approverId,
        CancellationToken cancellationToken);
}
