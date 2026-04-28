namespace HRM.Application.Common.Constants;

public static class WorkflowConstants
{
    /// <summary>Quy đổi giờ nghỉ/OT sang &quot;ngày công&quot; (8h = 1 ngày).</summary>
    public const decimal WorkHoursPerDay = 8m;

    /// <summary>Nếu tổng quy đổi ≤ ngưỡng này thì trưởng phòng duyệt xong (không cần giám đốc).</summary>
    public const decimal ManagerOnlyDayThreshold = 3m;
}
