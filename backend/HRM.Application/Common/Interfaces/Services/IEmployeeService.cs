using HRM.Application.Employees;

public interface IEmployeeService
{
    Task<int> CreateEmployeeAsync(CreateEmployeeCommand request, CancellationToken cancellationToken);
    Task UpdateEmployeeAsync(UpdateEmployeeCommand request, CancellationToken cancellationToken);
    Task DeleteEmployeeAsync(int employeeId, CancellationToken cancellationToken);
}