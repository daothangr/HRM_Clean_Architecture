using FluentValidation;
using HRM.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HRM.Application.Auth;

public record LoginCommand(string Email, string Password) : IRequest<AuthResponseDto>;

public class LoginCommandHandler : IRequestHandler<LoginCommand, AuthResponseDto>
{
    private readonly IEmployeeRepository _employeeRepo;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenService _jwt;

    public LoginCommandHandler(
        IEmployeeRepository employeeRepo,
        IPasswordHasher passwordHasher,
        IJwtTokenService jwt)
    {
        _employeeRepo = employeeRepo;
        _passwordHasher = passwordHasher;
        _jwt = jwt;
    }

    public async Task<AuthResponseDto> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        // Tìm nhân viên theo email, bao gồm cả thông tin về vai trò
        var employee= await _employeeRepo.GetEmployeeWithRolesByEmailAsync(request.Email, cancellationToken);
        if (employee == null)
            throw new UnauthorizedAccessException("Invalid email or password.");

        if (!employee.IsActive)
            throw new UnauthorizedAccessException("Tài khoản đã bị vô hiệu hóa.");

        if (!_passwordHasher.Verify(request.Password, employee.PasswordHash))
            throw new UnauthorizedAccessException("Invalid email or password.");

        var roles = employee.UserRoles.Select(ur => ur.Role.Name).ToList();
        var access = _jwt.CreateAccessToken(employee, roles);
        var refreshToken = _jwt.CreateRefreshToken();

        employee.RefreshToken = refreshToken;
        employee.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
        await _employeeRepo.SaveChangesAsync(cancellationToken);

        return new AuthResponseDto
        {
            AccessToken = access.Token,
            RefreshToken = refreshToken,
            ExpiresAt = access.ExpiresAtUtc,
            EmployeeId = employee.Id,
            EmployeeCode = employee.EmployeeCode,
            FullName = employee.FullName,
            Email = employee.Email,
            Roles = roles
        };
    }
}

public class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    public LoginCommandValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Password).NotEmpty();
    }
}
