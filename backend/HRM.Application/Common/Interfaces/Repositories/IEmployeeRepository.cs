using HRM.Application.Common.Interfaces.Repositories;
using HRM.Domain.Entities;

namespace HRM.Application.Common.Interfaces;

public interface IEmployeeRepository : IGenericRepository<Employee>
{
    Task<(List<Employee> Employees, int TotalCount)> GetEmployeesWithDepartmentPagedAsync(int pageNumber, int pageSize, CancellationToken cancellationToken);
    Task<(List<Employee> Employees, int TotalCount)> GetEmployeesByDepartmentPagedAsync(int departmentId, int pageNumber, int pageSize, CancellationToken cancellationToken);

    // Lấy thông tin nhân viên theo Id, kèm theo phòng ban và role
    Task<Employee?> GetEmployeeByIdWithDepartmentAndRolesAsync(int employeeId, CancellationToken cancellationToken);

    // Lấy Id phòng ban của nhân viên, dùng để kiểm tra quyền manager chỉ được xem nhân viên cùng phòng ban
    Task<int?> GetDepartmentIdByEmployeeIdAsync(int employeeId, CancellationToken cancellationToken);
    Task<int?> GetEmployeeIdByEmployeeCodeAsync(string employeeCode, CancellationToken cancellationToken);
    Task<bool> EmailExistsAsync(string email, int? excludeEmployeeId, CancellationToken cancellationToken);
    Task<bool> EmployeeCodeExistsAsync(string employeeCode, CancellationToken cancellationToken);
    Task<bool> EmployeeExistsAsync(int employeeId, CancellationToken cancellationToken);
    Task AddEmployeeAsync(Employee employee, CancellationToken cancellationToken);
    // Thay thế role của nhân viên bằng role mới (xóa role cũ và thêm role mới)
    Task ReplaceUserRoleAsync(Employee employee, int roleId, CancellationToken cancellationToken);
    Task AddUserRoleAsync(int userId, int roleId, CancellationToken cancellationToken);

    // Đảm bảo nhân viên có dữ liệu số ngày phép cho năm hiện tại, nếu chưa có thì tạo mới với số ngày phép mặc định (ví dụ: 12 ngày).
    Task EnsureLeaveBalanceForCurrentYearAsync(int employeeId, CancellationToken cancellationToken);
    Task<bool> IsEmployeeInDepartmentAsync(int employeeId, int departmentId, CancellationToken cancellationToken);
    Task<Employee?> GetEmployeeWithRolesByEmailAsync(string email, CancellationToken cancellationToken);
    Task<List<Employee>> GetEmployeesByRoleNameAsync(string roleName, CancellationToken cancellationToken);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
