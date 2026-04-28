using AutoMapper;
using HRM.Domain.Entities;
using HRM.Application.Auth;
using HRM.Application.Departments;
using HRM.Application.Employees;
using HRM.Application.WorkAttendance;
using HRM.Application.Leaves;

namespace HRM.Application;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<Employee, EmployeeDto>()
            .ForMember(d => d.DepartmentName, o => o.MapFrom(s => s.Department.Name))
            .ForMember(d => d.RoleName, o => o.MapFrom(s => s.UserRoles.Select(ur => ur.Role.Name)));
        CreateMap<Employee, EmployeeListDto>()
            .ForMember(d => d.DepartmentName, o => o.MapFrom(s => s.Department.Name));
        CreateMap<Department, DepartmentDto>();
        CreateMap<Attendance, AttendanceRecordDto>()
            .ForMember(d => d.EmployeeName, o => o.MapFrom(s => s.Employee.FullName))
            .ForMember(d => d.DepartmentName, o => o.MapFrom(s => s.Employee.Department.Name));
        CreateMap<LeaveRequest, LeaveRequestDto>()
            .ForMember(d => d.EmployeeName, o => o.MapFrom(s => s.Employee.FullName))
            .ForMember(d => d.DepartmentName, o => o.MapFrom(s => s.Employee.Department.Name));
    }
}
