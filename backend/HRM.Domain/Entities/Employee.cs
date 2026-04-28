using HRM.Domain.Enums;

namespace HRM.Domain.Entities;

public class Employee
{
    public int Id { get; set; }
    public string EmployeeCode { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public Gender? Gender { get; set; }
    public string? Address { get; set; }
    public int DepartmentId { get; set; }
    public Department Department { get; set; } = null!;
    public string? Position { get; set; }
    public int? ManagerId { get; set; }
    public Employee? Manager { get; set; }
    public ICollection<Employee> DirectReports { get; set; } = new List<Employee>();
    public DateTime? HireDate { get; set; }
    public DateTime? ResignDate { get; set; }
    public bool IsActive { get; set; } = true;
    public EmployeeStatus Status { get; set; } = EmployeeStatus.Active;
    public string PasswordHash { get; set; } = string.Empty;
    public string? RefreshToken { get; set; }
    public DateTime? RefreshTokenExpiryTime { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
    public ICollection<LeaveRequest> LeaveRequests { get; set; } = new List<LeaveRequest>();
    public ICollection<LeaveBalance> LeaveBalances { get; set; } = new List<LeaveBalance>();
    public ICollection<OvertimeRequest> OvertimeRequests { get; set; } = new List<OvertimeRequest>();
}
