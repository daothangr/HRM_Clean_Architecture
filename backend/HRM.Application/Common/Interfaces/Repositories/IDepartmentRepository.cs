using HRM.Application.Common.Interfaces.Repositories;
using HRM.Application.Departments;
using HRM.Domain.Entities;

namespace HRM.Application.Common.Interfaces;

public interface IDepartmentRepository : IGenericRepository<Department>
{
    // Lấy danh sách phòng ban, có thể bao gồm cả phòng ban không hoạt động nếu includeInactive = true
    Task<List<DepartmentDto>> GetDepartmentsAsync(bool includeInactive, CancellationToken cancellationToken);
    Task<bool> DepartmentCodeExistsAsync(string code, int? excludeDepartmentId, CancellationToken cancellationToken);
    Task<bool> DepartmentExistsAsync(int departmentId, CancellationToken cancellationToken);
    Task<bool> DepartmentHasEmployeesAsync(int departmentId, CancellationToken cancellationToken);
    Task AddDepartmentAsync(Department department, CancellationToken cancellationToken);
    Task<Department?> GetDepartmentByIdAsync(int departmentId, CancellationToken cancellationToken);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
