USE HRM_System;
GO

-- =====================================================
-- LEAVE REQUEST STORED PROCEDURES
-- =====================================================
-- This file contains all stored procedures for LeaveRequest
-- and ApprovalLog operations following the project's naming
-- convention: sp_[EntityName]_[Action]

-- =====================================================
-- 1. sp_LeaveRequests_GetLeaveRequestsWithEmployeeAndDepartment
-- Description: Get all leave requests with employee and department information
-- Returns: LeaveRequestDto result set with denormalized employee and department names
-- =====================================================
IF OBJECT_ID('dbo.sp_LeaveRequests_GetLeaveRequestsWithEmployeeAndDepartment', 'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_LeaveRequests_GetLeaveRequestsWithEmployeeAndDepartment;
GO

CREATE PROCEDURE dbo.sp_LeaveRequests_GetLeaveRequestsWithEmployeeAndDepartment
    @Status TINYINT = NULL,
    @PageNumber INT = 1,
    @PageSize INT = 10
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @Offset INT = (@PageNumber - 1) * @PageSize;
    
    -- Return total count
    SELECT COUNT(*) AS TotalCount
    FROM dbo.LeaveRequests lr
    INNER JOIN dbo.Employees e ON lr.EmployeeId = e.Id
    INNER JOIN dbo.Departments d ON e.DepartmentId = d.Id
    WHERE (@Status IS NULL OR lr.Status = @Status);
    
    -- Return paginated results
    SELECT 
        lr.Id,
        lr.EmployeeId,
        e.FullName AS EmployeeName,
        d.Name AS DepartmentName,
        lr.LeaveType,
        lr.StartDate,
        lr.EndDate,
        lr.IsFullDay,
        lr.StartTime,
        lr.EndTime,
        lr.TotalHours,
        lr.TotalDays,
        lr.Reason,
        lr.Status,
        lr.CurrentApprovalLevel,
        lr.CreatedAt
    FROM dbo.LeaveRequests lr
    INNER JOIN dbo.Employees e ON lr.EmployeeId = e.Id
    INNER JOIN dbo.Departments d ON e.DepartmentId = d.Id
    WHERE (@Status IS NULL OR lr.Status = @Status)
    ORDER BY lr.CreatedAt DESC
    OFFSET @Offset ROWS
    FETCH NEXT @PageSize ROWS ONLY;
END;
GO

-- =====================================================
-- 2. sp_LeaveRequests_GetLeaveRequestsByDepartmentId
-- Description: Get leave requests filtered by department
-- Parameters: @DepartmentId INT
-- Returns: LeaveRequestDto result set for specified department
-- =====================================================
IF OBJECT_ID('dbo.sp_LeaveRequests_GetLeaveRequestsByDepartmentId', 'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_LeaveRequests_GetLeaveRequestsByDepartmentId;
GO

CREATE PROCEDURE dbo.sp_LeaveRequests_GetLeaveRequestsByDepartmentId
    @DepartmentId INT,
    @Status TINYINT = NULL,
    @PageNumber INT = 1,
    @PageSize INT = 10
AS
BEGIN
    SET NOCOUNT ON;
    
    IF @DepartmentId <= 0
    BEGIN
        RAISERROR('DepartmentId must be greater than 0', 16, 1);
        RETURN;
    END;
    
    DECLARE @Offset INT = (@PageNumber - 1) * @PageSize;
    
    -- Return total count
    SELECT COUNT(*) AS TotalCount
    FROM dbo.LeaveRequests lr
    INNER JOIN dbo.Employees e ON lr.EmployeeId = e.Id
    INNER JOIN dbo.Departments d ON e.DepartmentId = d.Id
    WHERE d.Id = @DepartmentId AND (@Status IS NULL OR lr.Status = @Status);
    
    -- Return paginated results
    SELECT 
        lr.Id,
        lr.EmployeeId,
        e.FullName AS EmployeeName,
        d.Name AS DepartmentName,
        lr.LeaveType,
        lr.StartDate,
        lr.EndDate,
        lr.IsFullDay,
        lr.StartTime,
        lr.EndTime,
        lr.TotalHours,
        lr.TotalDays,
        lr.Reason,
        lr.Status,
        lr.CurrentApprovalLevel,
        lr.CreatedAt
    FROM dbo.LeaveRequests lr
    INNER JOIN dbo.Employees e ON lr.EmployeeId = e.Id
    INNER JOIN dbo.Departments d ON e.DepartmentId = d.Id
    WHERE d.Id = @DepartmentId AND (@Status IS NULL OR lr.Status = @Status)
    ORDER BY lr.CreatedAt DESC
    OFFSET @Offset ROWS
    FETCH NEXT @PageSize ROWS ONLY;
END;
GO

-- =====================================================
-- 3. sp_LeaveRequests_GetLeaveRequestsByEmployeeId
-- Description: Get leave requests filtered by employee
-- Parameters: @EmployeeId INT
-- Returns: LeaveRequestDto result set for specified employee
-- =====================================================
IF OBJECT_ID('dbo.sp_LeaveRequests_GetLeaveRequestsByEmployeeId', 'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_LeaveRequests_GetLeaveRequestsByEmployeeId;
GO

CREATE PROCEDURE dbo.sp_LeaveRequests_GetLeaveRequestsByEmployeeId
    @EmployeeId INT,
    @Status TINYINT = NULL,
    @PageNumber INT = 1,
    @PageSize INT = 10
AS
BEGIN
    SET NOCOUNT ON;

    IF @EmployeeId <= 0
    BEGIN
        RAISERROR('EmployeeId must be greater than 0', 16, 1);
        RETURN;
    END;

    DECLARE @Offset INT = (@PageNumber - 1) * @PageSize;

    -- Total Count
    SELECT COUNT(*) AS TotalCount
    FROM dbo.LeaveRequests lr
    INNER JOIN dbo.Employees e ON lr.EmployeeId = e.Id
    INNER JOIN dbo.Departments d ON e.DepartmentId = d.Id
    WHERE lr.EmployeeId = @EmployeeId
      AND (@Status IS NULL OR lr.Status = @Status);

    -- Paged Data
    SELECT 
        lr.Id,
        lr.EmployeeId,
        e.FullName AS EmployeeName,
        d.Name AS DepartmentName,
        lr.LeaveType,
        lr.StartDate,
        lr.EndDate,
        lr.IsFullDay,
        lr.StartTime,
        lr.EndTime,
        lr.TotalHours,
        lr.TotalDays,
        lr.Reason,
        lr.Status,
        lr.CurrentApprovalLevel,
        lr.CreatedAt
    FROM dbo.LeaveRequests lr
    INNER JOIN dbo.Employees e ON lr.EmployeeId = e.Id
    INNER JOIN dbo.Departments d ON e.DepartmentId = d.Id
    WHERE lr.EmployeeId = @EmployeeId
      AND (@Status IS NULL OR lr.Status = @Status)
    ORDER BY lr.CreatedAt DESC
    OFFSET @Offset ROWS
    FETCH NEXT @PageSize ROWS ONLY;
END;
GO

-- =====================================================
-- 4. sp_ApprovalLogs_AddApprovalLog
-- Description: Insert a new approval log entry
-- Parameters: 
--   @RequestId INT - The leave request being approved
--   @RequestType NVARCHAR(50) - Type of request (e.g., 'leave')
--   @ApproverId INT - Employee ID of the approver
--   @Action TINYINT - Approval action (1=Approved, 2=Rejected, 3=Forwarded)
--   @Comment NVARCHAR(MAX) - Optional comment from approver
--   @Level INT - Approval level
--   @CreatedAt DATETIME2 - Timestamp of the approval action
-- Returns: None (INSERT only)
-- =====================================================
IF OBJECT_ID('dbo.sp_ApprovalLogs_AddApprovalLog', 'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_ApprovalLogs_AddApprovalLog;
GO

CREATE PROCEDURE dbo.sp_ApprovalLogs_AddApprovalLog
    @RequestId INT,
    @RequestType NVARCHAR(50),
    @ApproverId INT,
    @Action TINYINT,
    @Comment NVARCHAR(MAX) = NULL,
    @Level INT,
    @CreatedAt DATETIME2
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Validation
    IF @RequestId <= 0
    BEGIN
        RAISERROR('RequestId must be greater than 0', 16, 1);
        RETURN;
    END;
    
    IF @ApproverId <= 0
    BEGIN
        RAISERROR('ApproverId must be greater than 0', 16, 1);
        RETURN;
    END;
    
    IF @Level <= 0
    BEGIN
        RAISERROR('Level must be greater than 0', 16, 1);
        RETURN;
    END;
    
    -- Verify leave request exists
    IF NOT EXISTS (SELECT 1 FROM dbo.LeaveRequests WHERE Id = @RequestId)
    BEGIN
        RAISERROR('LeaveRequest with specified Id does not exist', 16, 1);
        RETURN;
    END;
    
    -- Verify approver employee exists
    IF NOT EXISTS (SELECT 1 FROM dbo.Employees WHERE Id = @ApproverId)
    BEGIN
        RAISERROR('Approver employee with specified Id does not exist', 16, 1);
        RETURN;
    END;
    
    -- Insert approval log
    INSERT INTO dbo.ApprovalLogs 
    (
        RequestId,
        RequestType,
        ApproverId,
        Action,
        Comment,
        Level,
        CreatedAt
    )
    VALUES 
    (
        @RequestId,
        @RequestType,
        @ApproverId,
        @Action,
        @Comment,
        @Level,
        @CreatedAt
    );
    
END;
GO
