USE HRM_System;
GO

-- =====================================================
-- ATTENDANCE STORED PROCEDURES
-- =====================================================
-- This file contains all stored procedures for Attendance
-- operations following the project's naming convention:
-- sp_[EntityName]_[Action]

-- =====================================================
-- 1. sp_Attendance_CheckExistsByEmployeeAndDate
-- Description: Check if an attendance record exists for a specific employee on a given date
-- Parameters: 
--   @EmployeeId INT - Employee ID
--   @AttendanceDate DATE - The date to check (only date part, ignoring time)
-- Returns: INT (1 if exists, 0 if not exists)
-- =====================================================
IF OBJECT_ID('dbo.sp_Attendance_CheckExistsByEmployeeAndDate', 'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_Attendance_CheckExistsByEmployeeAndDate;
GO

CREATE PROCEDURE dbo.sp_Attendance_CheckExistsByEmployeeAndDate
    @EmployeeId INT,
    @AttendanceDate DATE
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Validation
    IF @EmployeeId <= 0
    BEGIN
        RAISERROR('EmployeeId must be greater than 0', 16, 1);
        RETURN;
    END;
    
    IF @AttendanceDate IS NULL
    BEGIN
        RAISERROR('AttendanceDate cannot be null', 16, 1);
        RETURN;
    END;
    
    -- Check if attendance record exists for this employee on the given date
    IF EXISTS (
        SELECT 1 
        FROM dbo.Attendances 
        WHERE EmployeeId = @EmployeeId 
          AND CAST(Date AS DATE) = @AttendanceDate
    )
    BEGIN
        SELECT 1 AS [Exists];
    END
    ELSE
    BEGIN
        SELECT 0 AS [Exists];
    END;
    
END;
GO

-- =====================================================
-- 2. sp_Attendance_GetAttendanceRecordsByEmployeePaged
-- Description: Get attendance records for an employee within a date range
-- Parameters: 
--   @From DATE - Start date
--   @To DATE - End date
--   @EmployeeId INT - Employee ID
-- Returns: List of attendance records with employee and department info
-- =====================================================
IF OBJECT_ID('dbo.sp_Attendance_GetAttendanceRecordsByEmployeePaged', 'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_Attendance_GetAttendanceRecordsByEmployeePaged;
GO

CREATE PROCEDURE dbo.sp_Attendance_GetAttendanceRecordsByEmployeePaged
    @From DATE,
    @To DATE,
    @EmployeeId INT,
    @PageNumber INT = 1,
    @PageSize INT = 10
AS
BEGIN
    SET NOCOUNT ON;

    IF @PageNumber < 1
        SET @PageNumber = 1;

    IF @PageSize < 1
        SET @PageSize = 20;

    SELECT
        a.Id,
        a.EmployeeId,
        e.FullName AS EmployeeName,
        d.Name AS DepartmentName,
        a.Date AS [Date],
        a.CheckInTime,
        a.CheckOutTime,
        a.WorkHours,
        a.OvertimeHours,
        a.Status
    FROM dbo.Attendances a
    INNER JOIN dbo.Employees e ON a.EmployeeId = e.Id
    INNER JOIN dbo.Departments d ON e.DepartmentId = d.Id
    WHERE a.Date >= @From
      AND a.Date <= @To
      AND a.EmployeeId = @EmployeeId
    ORDER BY
        a.Date DESC,
        e.FullName ASC
    OFFSET (@PageNumber - 1) * @PageSize ROWS
    FETCH NEXT @PageSize ROWS ONLY;

        SELECT COUNT(1) AS TotalCount
        FROM dbo.Attendances a
        INNER JOIN dbo.Employees e ON a.EmployeeId = e.Id
        WHERE a.Date >= @From
            AND a.Date <= @To
            AND a.EmployeeId = @EmployeeId;
END;
GO

-- =====================================================
-- 3. sp_Attendance_GetAttendanceByDeptAndEmployeePaged
-- Description: Get attendance records filtered by department and employee within a date range
-- Parameters: 
--   @From DATE - Start date
--   @To DATE - End date
--   @DeptId INT - Department ID
--   @EmployeeId INT - Employee ID
-- Returns: List of attendance records with department and employee info
-- =====================================================
IF OBJECT_ID('dbo.sp_Attendance_GetAttendanceByDeptAndEmployeePaged', 'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_Attendance_GetAttendanceByDeptAndEmployeePaged;
GO

CREATE PROCEDURE dbo.sp_Attendance_GetAttendanceByDeptAndEmployeePaged
    @From DATE,
    @To DATE,
    @DeptId INT,
    @EmployeeId INT,
    @PageNumber INT = 1,
    @PageSize INT = 20
AS
BEGIN
    SET NOCOUNT ON;

    IF @PageNumber < 1
        SET @PageNumber = 1;

    IF @PageSize < 1
        SET @PageSize = 20;

    SELECT
        a.Id,
        a.EmployeeId,
        e.FullName AS EmployeeName,
        d.Id AS DepartmentId,
        d.Name AS DepartmentName,
        a.Date AS [Date],
        a.CheckInTime,
        a.CheckOutTime,
        a.WorkHours,
        a.OvertimeHours,
        a.Status

    FROM dbo.Attendances a
    INNER JOIN dbo.Employees e ON a.EmployeeId = e.Id
    INNER JOIN dbo.Departments d ON e.DepartmentId = d.Id
    WHERE a.Date >= @From
      AND a.Date <= @To
      AND e.DepartmentId = @DeptId
      AND a.EmployeeId = @EmployeeId
    ORDER BY
        a.Date DESC,
        e.FullName ASC
        OFFSET (@PageNumber - 1) * @PageSize ROWS
        FETCH NEXT @PageSize ROWS ONLY;

        SELECT COUNT(1) AS TotalCount
        FROM dbo.Attendances a
        INNER JOIN dbo.Employees e ON a.EmployeeId = e.Id
        WHERE a.Date >= @From
            AND a.Date <= @To
            AND e.DepartmentId = @DeptId
            AND a.EmployeeId = @EmployeeId;
END;
GO

-- =====================================================
-- 4. sp_Attendance_GetByEmployeeAndDate
-- Description: Get attendance record by employee and date
-- Parameters:
--   @EmployeeId INT - Employee ID
--   @AttendanceDate DATE - Attendance date
-- Returns: One attendance record (or empty)
-- =====================================================
IF OBJECT_ID('dbo.sp_Attendance_GetByEmployeeAndDate', 'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_Attendance_GetByEmployeeAndDate;
GO

CREATE PROCEDURE dbo.sp_Attendance_GetByEmployeeAndDate
    @EmployeeId INT,
    @AttendanceDate DATE
AS
BEGIN
    SET NOCOUNT ON;

    SELECT TOP 1
        a.Id,
        a.EmployeeId,
        a.[Date],
        a.CheckInTime,
        a.CheckOutTime,
        a.WorkHours,
        a.OvertimeHours,
        a.Status
    FROM dbo.Attendances a
    WHERE a.EmployeeId = @EmployeeId
      AND CAST(a.[Date] AS DATE) = @AttendanceDate;
END;
GO
