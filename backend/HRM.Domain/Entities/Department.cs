using HRM.Domain.Enums;

namespace HRM.Domain.Entities;

public class Department
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int? ParentDepartmentId { get; set; }
    public Department? ParentDepartment { get; set; }
    public ICollection<Department> ChildDepartments { get; set; } = new List<Department>();
    public int? DepartmentHeadId { get; set; }
    public Employee? DepartmentHead { get; set; }
    public DepartmentStatus Status { get; set; } = DepartmentStatus.Active;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public ICollection<Employee> Employees { get; set; } = new List<Employee>();
}
