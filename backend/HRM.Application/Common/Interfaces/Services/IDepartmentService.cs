using HRM.Application.Departments;

namespace HRM.Application.Common.Interfaces;

public interface IDepartmentService
{
    Task<int> CreateDepartmentAsync(CreateDepartmentCommand request, CancellationToken cancellationToken);
    Task UpdateDepartmentAsync(UpdateDepartmentCommand request, CancellationToken cancellationToken);
    Task DeleteDepartmentAsync(int departmentId, CancellationToken cancellationToken);
}
