using HRM.Application.Common.Interfaces;
using HRM.Application.Departments;
using HRM.Domain.Entities;
using HRM.Domain.Enums;
using HRM.Domain.Exceptions;
using HRM.Domain.Interfaces;

namespace HRM.Infrastructure.Services;

public class DepartmentService : IDepartmentService
{
    private readonly IDepartmentRepository _departmentRepo;
    private readonly IEmployeeRepository _employeeRepo;
    private readonly IUnitOfWork _unitOfWork;

    public DepartmentService(
        IDepartmentRepository departmentRepo,
        IEmployeeRepository employeeRepo,
        IUnitOfWork unitOfWork)
    {
        _departmentRepo = departmentRepo;
        _employeeRepo = employeeRepo;
        _unitOfWork = unitOfWork;
    }

    public async Task<int> CreateDepartmentAsync(CreateDepartmentCommand request, CancellationToken cancellationToken)
    {
        await _unitOfWork.BeginTransactionAsync();

        try
        {
        if (await _departmentRepo.DepartmentCodeExistsAsync(request.Code, null, cancellationToken))
            throw new InvalidOperationException("Department code already exists.");

        if (request.ParentDepartmentId is { } pid &&
            !await _departmentRepo.DepartmentExistsAsync(pid, cancellationToken))
            throw new InvalidOperationException("Parent department not found.");

        if (request.DepartmentHeadId is { } hid &&
            !await _employeeRepo.EmployeeExistsAsync(hid, cancellationToken))
            throw new InvalidOperationException("Department head employee not found.");

        var department = new Department
        {
            Code = request.Code.Trim(),
            Name = request.Name.Trim(),
            ParentDepartmentId = request.ParentDepartmentId,
            DepartmentHeadId = request.DepartmentHeadId,
            Status = DepartmentStatus.Active,
            CreatedAt = DateTime.UtcNow
        };

        await _departmentRepo.AddAsync(department, cancellationToken);
            await _unitOfWork.CommitAsync();
        return department.Id;
        }
        catch
        {
            await _unitOfWork.RollbackAsync();
            throw;
        }
    }

    public async Task DeleteDepartmentAsync(int departmentId, CancellationToken cancellationToken)
    {
        await _unitOfWork.BeginTransactionAsync();

        try
        {
        var department = await _departmentRepo.GetDepartmentByIdAsync(departmentId, cancellationToken)
            ?? throw new NotFoundException(nameof(Department), departmentId);

        var hasEmployees = await _departmentRepo.DepartmentHasEmployeesAsync(department.Id, cancellationToken);
        if (hasEmployees)
            throw new InvalidOperationException("Cannot deactivate department while it still has employees.");

        department.Status = DepartmentStatus.Inactive;
        department.UpdatedAt = DateTime.UtcNow;
        await _departmentRepo.UpdateAsync(department, cancellationToken);
            await _unitOfWork.CommitAsync();
        }
        catch
        {
            await _unitOfWork.RollbackAsync();
            throw;
        }
    }

    public async Task UpdateDepartmentAsync(UpdateDepartmentCommand request, CancellationToken cancellationToken)
    {
        await _unitOfWork.BeginTransactionAsync();

        try
        {
            var department = await _departmentRepo.GetDepartmentByIdAsync(request.Id, cancellationToken)
                ?? throw new NotFoundException(nameof(Department), request.Id);

            if (await _departmentRepo.DepartmentCodeExistsAsync(request.Code, request.Id, cancellationToken))
                throw new InvalidOperationException("Department code already exists.");

            if (request.ParentDepartmentId == request.Id)
                throw new InvalidOperationException("Department cannot be its own parent.");

            if (request.ParentDepartmentId is { } parentId &&
                !await _departmentRepo.DepartmentExistsAsync(parentId, cancellationToken))
                throw new InvalidOperationException("Parent department not found.");

            if (request.DepartmentHeadId is { } headId &&
                !await _employeeRepo.EmployeeExistsAsync(headId, cancellationToken))
                throw new InvalidOperationException("Department head employee not found.");

            department.Code = request.Code.Trim();
            department.Name = request.Name.Trim();
            department.ParentDepartmentId = request.ParentDepartmentId;
            department.DepartmentHeadId = request.DepartmentHeadId;
            department.Status = request.Status;
            department.UpdatedAt = DateTime.UtcNow;

            await _departmentRepo.UpdateAsync(department, cancellationToken);
            await _unitOfWork.CommitAsync();
        }
        catch
        {
            await _unitOfWork.RollbackAsync();
            throw;
        }
    }
}
