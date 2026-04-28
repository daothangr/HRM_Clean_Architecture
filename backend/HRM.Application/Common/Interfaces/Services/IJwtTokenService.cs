using HRM.Domain.Entities;

namespace HRM.Application.Common.Interfaces;

public interface IJwtTokenService
{
    AccessTokenResult CreateAccessToken(Employee employee, IEnumerable<string> roles);
    string CreateRefreshToken();
}

public record AccessTokenResult(string Token, DateTime ExpiresAtUtc);
