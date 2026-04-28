namespace HRM.Domain.Entities;

public class LeaveBalance
{
    public int Id { get; set; }
    public int EmployeeId { get; set; }
    public Employee Employee { get; set; } = null!;
    public int Year { get; set; }
    public decimal AnnualLeave { get; set; } = 12;
    public decimal SickLeave { get; set; } = 3;
    public decimal UsedAnnual { get; set; }
    public decimal UsedSick { get; set; }
}
