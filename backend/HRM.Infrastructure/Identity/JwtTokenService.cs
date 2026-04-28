using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using HRM.Application.Common.Interfaces;
using HRM.Domain.Entities;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace HRM.Infrastructure.Identity;

public class JwtTokenService : IJwtTokenService
{
    private readonly JwtSettings _settings;

    public JwtTokenService(IOptions<JwtSettings> settings)
    {
        _settings = settings.Value;
    }

    public AccessTokenResult CreateAccessToken(Employee employee, IEnumerable<string> roles)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.Secret));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expires = DateTime.UtcNow.AddMinutes(_settings.ExpiresMinutes);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, employee.Id.ToString()),
            new(JwtRegisteredClaimNames.Sub, employee.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, employee.Email),
            new(ClaimTypes.Name, employee.FullName)
        };
        claims.AddRange(roles.Select(r => new Claim(ClaimTypes.Role, r)));

        var token = new JwtSecurityToken(
            issuer: _settings.Issuer,
            audience: _settings.Audience,
            claims: claims,
            expires: expires,
            signingCredentials: creds);

        var jwt = new JwtSecurityTokenHandler().WriteToken(token);
        return new AccessTokenResult(jwt, expires);
    }

    public string CreateRefreshToken()
    {
        var bytes = RandomNumberGenerator.GetBytes(32);
        return Convert.ToBase64String(bytes);
    }
}
