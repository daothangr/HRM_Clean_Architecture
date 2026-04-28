using HRM.Application.Common.Constants;
using HRM.Domain.Entities;

namespace HRM.Application.Common;

/// <summary>Shared rules for manager/director approval (leave, OT, attendance adjustment).</summary>
public static class WorkflowApproval
{
    public static bool CanApproveLevel1(
        HashSet<string> approverRoles,
        Employee approver,
        Employee requester)
    {
        if (approverRoles.Contains(SystemRoles.Admin))
            return true;
        if (approverRoles.Contains(SystemRoles.Director))
            return true;
        if (!approverRoles.Contains(SystemRoles.Manager))
            return false;

        if (requester.ManagerId == approver.Id)
            return true;

        return requester.Department.DepartmentHeadId == approver.Id;
    }
}
