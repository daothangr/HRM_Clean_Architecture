using HRM.Domain.Enums;

namespace HRM.Application.WorkAttendance;

public class AttendanceRecordDto
{
    public int Id { get; set; }
    public int EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public string DepartmentName { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public TimeOnly? CheckInTime { get; set; }
    public TimeOnly? CheckOutTime { get; set; }
    public decimal? WorkHours { get; set; }
    public decimal OvertimeHours { get; set; }
    public AttendanceStatus Status { get; set; }
}
