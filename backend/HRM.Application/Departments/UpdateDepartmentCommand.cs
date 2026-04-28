using FluentValidation;
using HRM.Application.Common.Constants;
using HRM.Application.Common.Interfaces;
using HRM.Domain.Enums;
using HRM.Domain.Exceptions;
using MediatR;

namespace HRM.Application.Departments;

public record UpdateDepartmentCommand(
    int Id,
    string Code,
    string Name,
    int? ParentDepartmentId,
    int? DepartmentHeadId,
    DepartmentStatus Status) : IRequest<Unit>;

public class UpdateDepartmentCommandHandler : IRequestHandler<UpdateDepartmentCommand, Unit>
{
    private readonly IDepartmentService _departmentService;
    private readonly ICurrentUserService _current;

    public UpdateDepartmentCommandHandler(
        IDepartmentService departmentService,
        ICurrentUserService current)
    {
        _departmentService = departmentService;
        _current = current;
    }

    public async Task<Unit> Handle(UpdateDepartmentCommand request, CancellationToken cancellationToken)
    {
        if (!_current.IsInRole(SystemRoles.Admin))
            throw new UnauthorizedAccessException();

        await _departmentService.UpdateDepartmentAsync(request, cancellationToken);
        return Unit.Value;
    }
}

public class UpdateDepartmentCommandValidator : AbstractValidator<UpdateDepartmentCommand>
{
    public UpdateDepartmentCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
        RuleFor(x => x.Code).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Name).NotEmpty().MaximumLength(255);
    }
}
