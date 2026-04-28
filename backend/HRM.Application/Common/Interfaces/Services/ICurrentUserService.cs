namespace HRM.Application.Common.Interfaces;

public interface ICurrentUserService
{
    int? UserId { get; }
    string? Email { get; }
    IReadOnlyList<string> Roles { get; }
    bool IsInRole(string role);
}
