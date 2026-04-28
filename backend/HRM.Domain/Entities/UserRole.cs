namespace HRM.Domain.Entities;

public class UserRole
{
    public int UserId { get; set; }
    public Employee User { get; set; } = null!;
    public int RoleId { get; set; }
    public Role Role { get; set; } = null!;
}
