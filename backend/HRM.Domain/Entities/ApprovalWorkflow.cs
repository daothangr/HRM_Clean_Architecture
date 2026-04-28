namespace HRM.Domain.Entities;

public class ApprovalWorkflow
{
    public int Id { get; set; }
    public string RequestType { get; set; } = string.Empty;
    public int? DepartmentId { get; set; }
    public Department? Department { get; set; }
    public int Level { get; set; }
    public byte ApproverRole { get; set; }
    public int? MaxDaysAllowed { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
}
