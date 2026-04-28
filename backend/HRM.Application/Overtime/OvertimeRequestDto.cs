using HRM.Domain.Enums;

namespace HRM.Application.Overtime;

public class OvertimeRequestDto
{
    public int Id { get; set; }
    public int EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public string DepartmentName { get; set; } = string.Empty;
    public DateTime WorkDate { get; set; }
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
    public decimal TotalHours { get; set; }
    public string? Reason { get; set; }
    public LeaveRequestStatus Status { get; set; }
    public int CurrentApprovalLevel { get; set; }
    public DateTime CreatedAt { get; set; }
}
