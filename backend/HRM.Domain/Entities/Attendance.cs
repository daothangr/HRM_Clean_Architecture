namespace HRM.Domain.Entities;

public class Attendance
{
    public int Id { get; set; }
    public int EmployeeId { get; set; }
    public Employee Employee { get; set; } = null!;
    public DateTime Date { get; set; }
    public TimeOnly? CheckInTime { get; set; }
    public TimeOnly? CheckOutTime { get; set; }
    public decimal? WorkHours { get; set; }
    public decimal OvertimeHours { get; set; }
    public byte Status { get; set; } = 1;
}
