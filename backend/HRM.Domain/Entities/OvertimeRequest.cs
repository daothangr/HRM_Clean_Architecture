using HRM.Domain.Enums;

namespace HRM.Domain.Entities;

/// <summary>Đăng ký làm thêm giờ (OT), duyệt theo cùng luồng manager → director khi vượt ngưỡng.</summary>
public class OvertimeRequest
{
    public int Id { get; set; }
    public int EmployeeId { get; set; }
    public Employee Employee { get; set; } = null!;
    public DateTime WorkDate { get; set; }
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
    public decimal TotalHours { get; set; }
    public string? Reason { get; set; }
    public LeaveRequestStatus Status { get; set; } = LeaveRequestStatus.Pending;
    public int CurrentApprovalLevel { get; set; } = 1;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
