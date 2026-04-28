using HRM.Application.Common.Interfaces;
using HRM.Application.Common.Security;
using MediatR;

namespace HRM.Application.Common.Behaviors;

public class AuthorizationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly ICurrentUserService _currentUserService;

    public AuthorizationBehavior(ICurrentUserService currentUserService)
    {
        _currentUserService = currentUserService;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (request is IAuthorizableRequest authorizableRequest && authorizableRequest.RequiredRoles.Count > 0)
        {
            var authorized = authorizableRequest.RequiredRoles.Any(_currentUserService.IsInRole);
            if (!authorized)
            {
                throw new UnauthorizedAccessException("Bạn không có quyền thực hiện yêu cầu này.");
            }
        }

        return await next();
    }
}
