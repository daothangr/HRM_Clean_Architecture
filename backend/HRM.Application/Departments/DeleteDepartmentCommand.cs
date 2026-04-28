using FluentValidation;
using HRM.Application.Common.Constants;
using HRM.Application.Common.Interfaces;
using MediatR;

namespace HRM.Application.Departments;

/// <summary>Ngừng sử dụng phòng ban (soft) nếu không còn nhân viên.</summary>
public record DeleteDepartmentCommand(int Id) : IRequest<Unit>;

public class DeleteDepartmentCommandHandler : IRequestHandler<DeleteDepartmentCommand, Unit>
{
    private readonly IDepartmentService _departmentService;
    private readonly ICurrentUserService _current;

    public DeleteDepartmentCommandHandler(IDepartmentService departmentService, ICurrentUserService current)
    {
        _departmentService = departmentService;
        _current = current;
    }

    public async Task<Unit> Handle(DeleteDepartmentCommand request, CancellationToken cancellationToken)
    {
        if (!_current.IsInRole(SystemRoles.Admin))
            throw new UnauthorizedAccessException();

        await _departmentService.DeleteDepartmentAsync(request.Id, cancellationToken);
        return Unit.Value;
    }
}

public class DeleteDepartmentCommandValidator : AbstractValidator<DeleteDepartmentCommand>
{
    public DeleteDepartmentCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
    }
}
