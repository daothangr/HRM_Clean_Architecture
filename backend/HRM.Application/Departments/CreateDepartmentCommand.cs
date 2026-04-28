using FluentValidation;
using HRM.Application.Common.Constants;
using HRM.Application.Common.Interfaces;
using MediatR;

namespace HRM.Application.Departments;

public record CreateDepartmentCommand(
    string Code,
    string Name,
    int? ParentDepartmentId,
    int? DepartmentHeadId) : IRequest<int>;

public class CreateDepartmentCommandHandler : IRequestHandler<CreateDepartmentCommand, int>
{
    private readonly IDepartmentService _departmentService;
    private readonly ICurrentUserService _current;

    public CreateDepartmentCommandHandler(IDepartmentService departmentService, ICurrentUserService current)
    {
        _departmentService = departmentService;
        _current = current;
    }

    public async Task<int> Handle(CreateDepartmentCommand request, CancellationToken cancellationToken)
    {
        if (!_current.IsInRole(SystemRoles.Admin))
            throw new UnauthorizedAccessException();

        return await _departmentService.CreateDepartmentAsync(request, cancellationToken);
    }
}

public class CreateDepartmentCommandValidator : AbstractValidator<CreateDepartmentCommand>
{
    public CreateDepartmentCommandValidator()
    {
        RuleFor(x => x.Code).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Name).NotEmpty().MaximumLength(255);
    }
}
