namespace HRM.Application.Common.Security;

public interface IAuthorizableRequest
{
    IReadOnlyCollection<string> RequiredRoles { get; }
}
