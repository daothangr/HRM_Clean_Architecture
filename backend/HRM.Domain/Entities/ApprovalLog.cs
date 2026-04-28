using HRM.Domain.Enums;

namespace HRM.Domain.Entities;

public class ApprovalLog
{
    public int Id { get; set; }
    public int RequestId { get; set; }
    public string RequestType { get; set; } = "leave";
    public int ApproverId { get; set; }
    public Employee Approver { get; set; } = null!;
    public ApprovalAction Action { get; set; }
    public string? Comment { get; set; }
    public int Level { get; set; }
    public DateTime CreatedAt { get; set; }
}
