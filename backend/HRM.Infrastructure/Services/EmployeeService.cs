using HRM.Application.Common.Interfaces;
using HRM.Application.Common.Constants;
using HRM.Application.Common.Utils;
using HRM.Application.Employees;
using HRM.Domain.Entities;
using HRM.Domain.Enums;
using HRM.Domain.Exceptions;
using HRM.Domain.Interfaces;

public class EmployeeService : IEmployeeService
{
    private readonly IEmployeeRepository _employeeRepo;
    private readonly IDepartmentRepository _departmentRepo;
    private readonly IRoleRepository _roleRepo;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IUnitOfWork _unitOfWork;

    public EmployeeService(
        IEmployeeRepository employeeRepo,
        IDepartmentRepository departmentRepo,
        IRoleRepository roleRepo,
        IPasswordHasher passwordHasher,
        IUnitOfWork unitOfWork)
    {
        _employeeRepo = employeeRepo;
        _departmentRepo = departmentRepo;
        _roleRepo = roleRepo;
        _passwordHasher = passwordHasher;
        _unitOfWork = unitOfWork;
    }

    public async Task<int> CreateEmployeeAsync(CreateEmployeeCommand request, CancellationToken cancellationToken)
{
    // Validate
    if (await _employeeRepo.EmailExistsAsync(request.Email, null, cancellationToken))
        throw new InvalidOperationException("Email already exists.");

    if (await _employeeRepo.EmployeeCodeExistsAsync(request.EmployeeCode, cancellationToken))
        throw new InvalidOperationException("Employee code already exists.");

    if (!await _departmentRepo.DepartmentExistsAsync(request.DepartmentId, cancellationToken))
        throw new InvalidOperationException("Department not found.");

    var role = await _roleRepo.GetRoleByNameAsync(request.RoleName, cancellationToken)
        ?? throw new InvalidOperationException($"Role '{request.RoleName}' not found.");

    if (!TemporalUtils.IsSqlDateValid(request.HireDate))
        throw new InvalidOperationException("HireDate must be >= 1753-01-01.");

    // Create entity
    var employee = new Employee
    {
        EmployeeCode = request.EmployeeCode,
        FullName = request.FullName,
        Gender = request.Gender,
        Email = request.Email,
        Phone = request.Phone,
        DepartmentId = request.DepartmentId,
        Position = request.Position,
        ManagerId = request.ManagerId,
        HireDate = TemporalUtils.NormalizeSqlDate(request.HireDate, dateOnly: true),
        Status = EmployeeStatus.Active,
        PasswordHash = _passwordHasher.Hash(request.Password),
        CreatedAt = DateTime.UtcNow
    };

    // TRANSACTION
    await _unitOfWork.BeginTransactionAsync();

    try
    {
        await _employeeRepo.AddAsync(employee, cancellationToken);

        await _employeeRepo.AddUserRoleAsync(employee.Id, role.Id, cancellationToken);

        await _employeeRepo.EnsureLeaveBalanceForCurrentYearAsync(employee.Id, cancellationToken);

        // COMMIT
        await _unitOfWork.CommitAsync(); 
    }
    catch
    {
        // ROLLBACK nếu lỗi
        await _unitOfWork.RollbackAsync();
        throw;
    }

    return employee.Id;
}

    public async Task UpdateEmployeeAsync(UpdateEmployeeCommand request, CancellationToken cancellationToken)
    {
        var employee = await _employeeRepo.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Employee), request.Id);

        if (await _employeeRepo.EmailExistsAsync(request.Email, request.Id, cancellationToken))
            throw new InvalidOperationException("Email already exists.");

        if (!await _departmentRepo.DepartmentExistsAsync(request.DepartmentId, cancellationToken))
            throw new InvalidOperationException("Department not found.");

        if (request.ManagerId is { } mid)
        {
            if (mid == request.Id)
                throw new InvalidOperationException("Employee cannot be their own manager.");

            if (!await _employeeRepo.EmployeeExistsAsync(mid, cancellationToken))
                throw new InvalidOperationException("Manager not found.");
        }

        var role = await _roleRepo.GetRoleByNameAsync(request.RoleName, cancellationToken)
            ?? throw new InvalidOperationException($"Role '{request.RoleName}' not found.");

        if (!TemporalUtils.IsSqlDateValid(request.HireDate))
            throw new InvalidOperationException("HireDate must be >= 1753-01-01.");

        var normalizedDateOfBirth = TemporalUtils.NormalizeSqlDate(request.DateOfBirth, dateOnly: true);
        var normalizedResignDate = TemporalUtils.NormalizeSqlDate(request.ResignDate, dateOnly: true);

        // Update employee properties
        employee.FullName = request.FullName.Trim();
        employee.Email = request.Email.Trim();
        employee.Phone = string.IsNullOrWhiteSpace(request.Phone) ? null : request.Phone.Trim();
        employee.DepartmentId = request.DepartmentId;
        employee.Position = string.IsNullOrWhiteSpace(request.Position) ? null : request.Position.Trim();
        employee.ManagerId = request.ManagerId;
        employee.DateOfBirth = normalizedDateOfBirth;
        employee.Gender = request.Gender;
        employee.Address = string.IsNullOrWhiteSpace(request.Address) ? null : request.Address.Trim();
        employee.HireDate = TemporalUtils.NormalizeSqlDate(request.HireDate, dateOnly: true);
        employee.ResignDate = normalizedResignDate;
        employee.Status = request.Status;
        employee.UpdatedAt = DateTime.UtcNow;

        if (!string.IsNullOrEmpty(request.NewPassword))
            employee.PasswordHash = _passwordHasher.Hash(request.NewPassword!);

        // TRANSACTION to ensure both Employee and Role updates are atomic
        await _unitOfWork.BeginTransactionAsync();

        try
        {
            // Update employee in database
            await _employeeRepo.UpdateAsync(employee, cancellationToken);

            // Replace user role
            await _employeeRepo.ReplaceUserRoleAsync(employee, role.Id, cancellationToken);

            // COMMIT transaction
            await _unitOfWork.CommitAsync();
        }
        catch
        {
            // ROLLBACK if error occurs
            await _unitOfWork.RollbackAsync();
            throw;
        }
    }

    public async Task DeleteEmployeeAsync(int employeeId, CancellationToken cancellationToken)
    {
        var employee = await _employeeRepo.GetByIdAsync(employeeId, cancellationToken)
            ?? throw new NotFoundException(nameof(Employee), employeeId);

        if (!employee.IsActive)
            return;

        employee.IsActive = false;
        employee.Status = EmployeeStatus.Resigned;
        employee.RefreshToken = null;
        employee.RefreshTokenExpiryTime = null;
        employee.ResignDate ??= DateTime.UtcNow.Date;
        employee.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.BeginTransactionAsync();
        try
        {
            await _employeeRepo.UpdateAsync(employee, cancellationToken);
            await _unitOfWork.CommitAsync();
        }
        catch
        {
            await _unitOfWork.RollbackAsync();
            throw;
        }
    }

}