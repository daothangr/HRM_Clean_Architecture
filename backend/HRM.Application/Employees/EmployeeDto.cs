using HRM.Domain.Enums;

namespace HRM.Application.Employees;

public class EmployeeDto
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
    public string DepartmentName { get; set; } = string.Empty;
    public string? Position { get; set; }
    public int? ManagerId { get; set; }
    public DateTime HireDate { get; set; }
    public EmployeeStatus Status { get; set; }
    public IReadOnlyList<string> RoleName { get; set; } = Array.Empty<string>();
}

public class EmployeeListDto
{
    public int Id { get; set; }
    public string EmployeeCode { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string DepartmentName { get; set; } = string.Empty;
}
