using FluentValidation;
using HRM.Application.Common.Constants;
using HRM.Application.Common.Interfaces;
using HRM.Domain.Enums;
using MediatR;

namespace HRM.Application.Employees;

public record UpdateEmployeeCommand(
    int Id,
    string FullName,
    string Email,
    string? Phone,
    int DepartmentId,
    string? Position,
    int? ManagerId,
    DateTime? DateOfBirth,
    Gender? Gender,
    string? Address,
    DateTime HireDate,
    DateTime? ResignDate,
    EmployeeStatus Status,
    string RoleName,
    string? NewPassword) : IRequest<Unit>;

public class UpdateEmployeeCommandHandler : IRequestHandler<UpdateEmployeeCommand, Unit>
{
    private readonly IEmployeeService _employeeService;
    private readonly ICurrentUserService _current;

    public UpdateEmployeeCommandHandler(
        IEmployeeService employeeService,
        ICurrentUserService current)
    {
        _employeeService = employeeService;
        _current = current;
    }

    public async Task<Unit> Handle(UpdateEmployeeCommand request, CancellationToken cancellationToken)
    {
        if (!_current.IsInRole(SystemRoles.Admin))
            throw new UnauthorizedAccessException();

        await _employeeService.UpdateEmployeeAsync(request, cancellationToken);
        return Unit.Value;
    }
}

public class UpdateEmployeeCommandValidator : AbstractValidator<UpdateEmployeeCommand>
{
    public UpdateEmployeeCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
        RuleFor(x => x.FullName).NotEmpty().MaximumLength(255);
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.DepartmentId).GreaterThan(0);
        RuleFor(x => x.RoleName).NotEmpty();
        RuleFor(x => x.NewPassword).MinimumLength(6).When(x => !string.IsNullOrEmpty(x.NewPassword));
        RuleFor(x => x.HireDate)
            .GreaterThanOrEqualTo(DateTimeConstants.SqlMinDate)
            .WithMessage("HireDate must be >= 1753-01-01.");
        RuleFor(x => x.DateOfBirth)
            .Must(d => !d.HasValue || d.Value >= DateTimeConstants.SqlMinDate)
            .WithMessage("DateOfBirth must be >= 1753-01-01 when provided.");
        RuleFor(x => x.ResignDate)
            .Must(d => !d.HasValue || d.Value >= DateTimeConstants.SqlMinDate)
            .WithMessage("ResignDate must be >= 1753-01-01 when provided.");
    }
}
