using HRM.Domain.Enums;

namespace HRM.Application.Departments;

public class DepartmentDto
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int? ParentDepartmentId { get; set; }
    public int? DepartmentHeadId { get; set; }
    public DepartmentStatus Status { get; set; }
}
