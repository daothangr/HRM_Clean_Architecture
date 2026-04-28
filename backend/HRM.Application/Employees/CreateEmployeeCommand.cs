using FluentValidation;
using HRM.Application.Common.Constants;
using HRM.Application.Common.Interfaces;
using HRM.Domain.Entities;
using HRM.Domain.Enums;
using MediatR;

namespace HRM.Application.Employees;

public record CreateEmployeeCommand(
    string EmployeeCode,
    Gender Gender,
    string FullName,
    string Email,
    string Password,
    string? Phone,
    int DepartmentId,
    string? Position,
    int? ManagerId,
    DateTime HireDate,
    string RoleName) : IRequest<int>;

public class CreateEmployeeCommandHandler : IRequestHandler<CreateEmployeeCommand, int>
{
    private readonly IEmployeeService _service;
    private readonly ICurrentUserService _current;

    public CreateEmployeeCommandHandler(IEmployeeService service, ICurrentUserService current)
    {
        _service = service;
        _current = current;
    }

    public async Task<int> Handle(CreateEmployeeCommand request, CancellationToken cancellationToken)
    {
        if (!_current.IsInRole(SystemRoles.Admin))
            throw new UnauthorizedAccessException();

        return await _service.CreateEmployeeAsync(request, cancellationToken);
    }
}

public class CreateEmployeeCommandValidator : AbstractValidator<CreateEmployeeCommand>
{
    public CreateEmployeeCommandValidator()
    {
        RuleFor(x => x.EmployeeCode).NotEmpty().MaximumLength(50);
        RuleFor(x => x.FullName).NotEmpty().MaximumLength(255);
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Password).MinimumLength(6);
        RuleFor(x => x.DepartmentId).GreaterThan(0);
        RuleFor(x => x.RoleName).NotEmpty();
        RuleFor(x => x.HireDate)
            .GreaterThanOrEqualTo(DateTimeConstants.SqlMinDate)
            .WithMessage("HireDate must be >= 1753-01-01.");
    }
}
