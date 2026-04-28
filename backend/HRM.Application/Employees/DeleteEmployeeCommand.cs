using FluentValidation;
using HRM.Application.Common.Constants;
using HRM.Application.Common.Interfaces;
using MediatR;

namespace HRM.Application.Employees;

public record DeleteEmployeeCommand(int Id) : IRequest<Unit>;

public class DeleteEmployeeCommandHandler : IRequestHandler<DeleteEmployeeCommand, Unit>
{
    private readonly IEmployeeService _employeeService;
    private readonly ICurrentUserService _current;

    public DeleteEmployeeCommandHandler(IEmployeeService employeeService, ICurrentUserService current)
    {
        _employeeService = employeeService;
        _current = current;
    }

    public async Task<Unit> Handle(DeleteEmployeeCommand request, CancellationToken cancellationToken)
    {
        if (!_current.IsInRole(SystemRoles.Admin))
            throw new UnauthorizedAccessException();

        await _employeeService.DeleteEmployeeAsync(request.Id, cancellationToken);
        return Unit.Value;
    }
}

public class DeleteEmployeeCommandValidator : AbstractValidator<DeleteEmployeeCommand>
{
    public DeleteEmployeeCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
    }
}
