using HRM.Domain.Enums;

namespace HRM.Domain.Entities;

public class LeaveRequest
{
    public int Id { get; set; }
    public int EmployeeId { get; set; }
    public Employee Employee { get; set; } = null!;
    public LeaveType LeaveType { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    /// <summary>True: nghỉ cả ngày (theo khoảng ngày làm việc). False: nghỉ theo giờ trong một ngày.</summary>
    public bool IsFullDay { get; set; } = true;
    /// <summary>Áp dụng khi <see cref="IsFullDay"/> = false (cùng ngày <see cref="StartDate"/>).</summary>
    public TimeOnly? StartTime { get; set; }
    public TimeOnly? EndTime { get; set; }
    /// <summary>Tổng giờ nghỉ (khi nghỉ theo giờ). Null khi nghỉ cả ngày.</summary>
    public decimal? TotalHours { get; set; }
    public decimal TotalDays { get; set; }
    public string? Reason { get; set; }
    public LeaveRequestStatus Status { get; set; } = LeaveRequestStatus.Pending;
    public int CurrentApprovalLevel { get; set; } = 1;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
