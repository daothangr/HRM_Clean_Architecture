using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using HRM.Application.Common.Interfaces;
using Microsoft.AspNetCore.Http;

namespace HRM.Infrastructure.Identity;

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _http;

    public CurrentUserService(IHttpContextAccessor http)
    {
        _http = http;
    }

    public int? UserId =>
        int.TryParse(_http.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier), out var id)
            ? id
            : null;

    public string? Email =>
        _http.HttpContext?.User?.FindFirstValue(JwtRegisteredClaimNames.Email)
        ?? _http.HttpContext?.User?.FindFirstValue(ClaimTypes.Email);

    public IReadOnlyList<string> Roles =>
        _http.HttpContext?.User?.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList()
        ?? (IReadOnlyList<string>)Array.Empty<string>();

    public bool IsInRole(string role) =>
        _http.HttpContext?.User?.IsInRole(role) == true;
}
