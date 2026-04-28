USE HRM_System;
GO
-- =====================================================
-- EMPLOYEE SERVICE STORED PROCEDURES
-- =====================================================
-- This file contains all stored procedures for Employee
-- operations following the project's naming convention:
-- sp_[EntityName]_[Action]

-- =====================================================
-- 1. sp_Employees_GetEmployeesWithDepartment
-- Description: Get all employees with their department information
-- Returns: List of employees with department details
-- =====================================================
IF OBJECT_ID('dbo.sp_Employees_GetEmployeesWithDepartment', 'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_Employees_GetEmployeesWithDepartment;
GO

CREATE PROCEDURE dbo.sp_Employees_GetEmployeesWithDepartment
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        e.Id,
        e.EmployeeCode,
        e.FullName,
        e.Email,
        e.Phone,
        e.DateOfBirth,
        e.Gender,
        e.Address,
        d.Id AS DepartmentId,
        d.Name AS DepartmentName,
        e.Position,
        e.ManagerId,
        e.HireDate,
        e.ResignDate,
        e.IsActive,
        e.Status,
        e.CreatedAt,
        e.UpdatedAt
    FROM dbo.Employees e
    INNER JOIN dbo.Departments d ON e.DepartmentId = d.Id
    WHERE e.IsActive = 1
    ORDER BY e.FullName;
END;
GO

-- =====================================================
-- 2. sp_Employees_GetEmployeesByDepartment
-- Description: Get employees filtered by department ID
-- Parameters: @DepartmentId INT
-- Returns: List of employees in the specified department
-- =====================================================
IF OBJECT_ID('dbo.sp_Employees_GetEmployeesByDepartment', 'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_Employees_GetEmployeesByDepartment;
GO

CREATE PROCEDURE dbo.sp_Employees_GetEmployeesByDepartment
    @DepartmentId INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT 
        e.Id,
        e.EmployeeCode,
        e.FullName,
        e.Email,
        e.Phone,
        e.DateOfBirth,
        e.Gender,
        e.Address,
        d.Id AS DepartmentId,
        d.Name AS DepartmentName,
        e.Position,
        e.ManagerId,
        e.HireDate,
        e.ResignDate,
        e.IsActive,
        e.Status,
        e.CreatedAt,
        e.UpdatedAt
    FROM dbo.Employees e
    INNER JOIN dbo.Departments d ON e.DepartmentId = d.Id
        WHERE e.DepartmentId = @DepartmentId
            AND e.IsActive = 1
    ORDER BY e.FullName;
END;
GO

-- =====================================================
-- 3. sp_Employees_GetEmployeeByIdWithDepartmentAndRoles
-- Description: Get a specific employee with department and all assigned roles
-- Parameters: @EmployeeId INT
-- Returns: 
--   First result set: Employee with department
--   Second result set: List of assigned roles
-- =====================================================
IF OBJECT_ID('dbo.sp_Employees_GetEmployeeByIdWithDepartmentAndRoles', 'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_Employees_GetEmployeeByIdWithDepartmentAndRoles;
GO

CREATE PROCEDURE dbo.sp_Employees_GetEmployeeByIdWithDepartmentAndRoles
    @EmployeeId INT
AS
BEGIN
    SET NOCOUNT ON;

    -- =========================
    -- 1. Employee
    -- =========================
    SELECT 
        Id,
        EmployeeCode,
        FullName,
        Email,
        Phone,
        DateOfBirth,
        Gender,
        Address,
        DepartmentId,
        Position,
        ManagerId,
        HireDate,
        ResignDate,
        Status,
        CreatedAt,
        UpdatedAt
    FROM dbo.Employees
    WHERE Id = @EmployeeId;

    -- =========================
    -- 2. Department
    -- =========================
    SELECT 
        d.Id,
        d.Name
    FROM dbo.Departments d
    INNER JOIN dbo.Employees e ON e.DepartmentId = d.Id
    WHERE e.Id = @EmployeeId;

    -- =========================
    -- 3. Roles
    -- =========================
    SELECT 
        r.Id,
        r.Name,
        r.Description
    FROM dbo.UserRoles ur
    INNER JOIN dbo.Roles r ON ur.RoleId = r.Id
    WHERE ur.UserId = @EmployeeId;
END;
GO

-- =====================================================
-- 4. sp_Employees_GetDepartmentIdByEmployeeId
-- Description: Get department ID for a specific employee
-- Parameters: @EmployeeId INT
-- Returns: DepartmentId
-- =====================================================
IF OBJECT_ID('dbo.sp_Employees_GetDepartmentIdByEmployeeId', 'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_Employees_GetDepartmentIdByEmployeeId;
GO

CREATE PROCEDURE dbo.sp_Employees_GetDepartmentIdByEmployeeId
    @EmployeeId INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT DepartmentId
    FROM dbo.Employees
    WHERE Id = @EmployeeId;
END;
GO

-- =====================================================
-- 5. sp_Employees_CheckEmailExists
-- Description: Check if an email already exists for an employee
-- Parameters: 
--   @Email NVARCHAR(255)
--   @ExcludeEmployeeId INT (optional, for update scenarios)
-- Returns: INT (1 if exists, 0 if not exists)
-- =====================================================
IF OBJECT_ID('dbo.sp_Employees_CheckEmailExists', 'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_Employees_CheckEmailExists;
GO

CREATE PROCEDURE dbo.sp_Employees_CheckEmailExists
    @Email NVARCHAR(255),
    @ExcludeEmployeeId INT = NULL
AS
BEGIN
    SET NOCOUNT ON;

    SELECT CASE 
        WHEN EXISTS (
            SELECT 1
            FROM dbo.Employees
            WHERE Email = LTRIM(RTRIM(@Email))
              AND (@ExcludeEmployeeId IS NULL OR Id <> @ExcludeEmployeeId)
        )
        THEN 1 ELSE 0
    END AS IsExists;
END;
GO

-- =====================================================
-- 6. sp_Employees_CheckEmployeeCodeExists
-- Description: Check if an employee code already exists
-- Parameters: @EmployeeCode NVARCHAR(50)
-- Returns: INT (1 if exists, 0 if not exists)
-- =====================================================
IF OBJECT_ID('dbo.sp_Employees_CheckEmployeeCodeExists', 'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_Employees_CheckEmployeeCodeExists;
GO

CREATE PROCEDURE dbo.sp_Employees_CheckEmployeeCodeExists
    @EmployeeCode NVARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT CASE 
        WHEN EXISTS (
            SELECT 1
            FROM dbo.Employees
            WHERE EmployeeCode = LTRIM(RTRIM(@EmployeeCode))
        )
        THEN 1 ELSE 0
    END AS IsExists;
END;
GO

-- =====================================================
-- 7. sp_Employees_CheckEmployeeExists
-- Description: Check if an employee exists by ID
-- Parameters: @EmployeeId INT
-- Returns: INT (1 if exists, 0 if not exists)
-- =====================================================
IF OBJECT_ID('dbo.sp_Employees_CheckEmployeeExists', 'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_Employees_CheckEmployeeExists;
GO

CREATE PROCEDURE dbo.sp_Employees_CheckEmployeeExists
    @EmployeeId INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT CASE 
        WHEN EXISTS (
            SELECT 1
            FROM dbo.Employees
            WHERE Id = @EmployeeId
        )
        THEN 1 ELSE 0
    END AS IsExists;
END;
GO

-- =====================================================
-- 8. sp_Employees_GetRoleByName
-- Description: Get a role by its name
-- Parameters: @RoleName NVARCHAR(100)
-- Returns: Role details (first matching role)
-- =====================================================
IF OBJECT_ID('dbo.sp_Employees_GetRoleByName', 'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_Employees_GetRoleByName;
GO

CREATE PROCEDURE dbo.sp_Employees_GetRoleByName
    @RoleName NVARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT TOP 1 *
    FROM dbo.Roles
    WHERE Name = @RoleName;
END;
GO

-- =====================================================
-- 9. sp_Employees_AddEmployee
-- Description: Create a new employee record
-- Parameters: 
--   @EmployeeCode NVARCHAR(50)
--   @FullName NVARCHAR(200)
--   @Email NVARCHAR(255)
--   @Phone NVARCHAR(50)
--   @DepartmentId INT
--   @Position NVARCHAR(100)
--   @HireDate DATE
--   @PasswordHash NVARCHAR(255)
-- Returns: NewId (SCOPE_IDENTITY)
-- =====================================================
IF OBJECT_ID('dbo.sp_Employees_AddEmployee', 'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_Employees_AddEmployee;
GO

CREATE PROCEDURE dbo.sp_Employees_AddEmployee
    @EmployeeCode NVARCHAR(50),
    @FullName NVARCHAR(200),
    @Email NVARCHAR(255),
    @Phone NVARCHAR(50),
    @DepartmentId INT,
    @Position NVARCHAR(100),
    @HireDate DATE,
    @PasswordHash NVARCHAR(255)
AS
BEGIN
    SET NOCOUNT ON;

    INSERT INTO dbo.Employees
    (
        EmployeeCode,
        FullName,
        Email,
        Phone,
        DepartmentId,
        Position,
        HireDate,
        PasswordHash,
        Status,
        CreatedAt
    )
    VALUES
    (
        @EmployeeCode,
        @FullName,
        @Email,
        @Phone,
        @DepartmentId,
        @Position,
        @HireDate,
        @PasswordHash,
        1,
        GETUTCDATE()
    );

    SELECT SCOPE_IDENTITY() AS NewId;
END;
GO

-- =====================================================
-- 10. sp_Employees_AddUserRole
-- Description: Assign a role to an employee
-- Parameters: 
--   @UserId INT (Employee ID)
--   @RoleId INT
-- Returns: None (INSERT only)
-- =====================================================
IF OBJECT_ID('dbo.sp_Employees_AddUserRole', 'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_Employees_AddUserRole;
GO

CREATE PROCEDURE dbo.sp_Employees_AddUserRole
    @UserId INT,
    @RoleId INT
AS
BEGIN
    SET NOCOUNT ON;

    INSERT INTO dbo.UserRoles(UserId, RoleId)
    VALUES (@UserId, @RoleId);
END;
GO

-- =====================================================
-- 11. sp_Employees_ReplaceUserRole
-- Description: Replace employee's role (remove old, assign new)
-- Parameters: 
--   @UserId INT (Employee ID)
--   @RoleId INT (New role ID)
-- Returns: None
-- =====================================================
IF OBJECT_ID('dbo.sp_Employees_ReplaceUserRole', 'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_Employees_ReplaceUserRole;
GO

CREATE PROCEDURE dbo.sp_Employees_ReplaceUserRole
    @UserId INT,
    @RoleId INT
AS
BEGIN
    SET NOCOUNT ON;

    DELETE FROM dbo.UserRoles WHERE UserId = @UserId;

    INSERT INTO dbo.UserRoles(UserId, RoleId)
    VALUES (@UserId, @RoleId);
END;
GO

-- =====================================================
-- 12. sp_Employees_EnsureLeaveBalance
-- Description: Ensure a leave balance record exists for the current year
-- Parameters: @EmployeeId INT
-- Returns: None
-- =====================================================
IF OBJECT_ID('dbo.sp_Employees_EnsureLeaveBalance', 'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_Employees_EnsureLeaveBalance;
GO

CREATE PROCEDURE dbo.sp_Employees_EnsureLeaveBalance
    @EmployeeId INT
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @Year INT = YEAR(GETUTCDATE());

    IF NOT EXISTS (
        SELECT 1
        FROM dbo.LeaveBalances
        WHERE EmployeeId = @EmployeeId AND Year = @Year
    )
    BEGIN
        INSERT INTO dbo.LeaveBalances(EmployeeId, Year)
        VALUES (@EmployeeId, @Year);
    END
END;
GO

-- =====================================================
-- 13. sp_Employees_IsEmployeeInDepartment
-- Description: Check if an employee belongs to a specific department
-- Parameters: 
--   @EmployeeId INT
--   @DepartmentId INT
-- Returns: INT (1 if true, 0 if false)
-- =====================================================
IF OBJECT_ID('dbo.sp_Employees_IsEmployeeInDepartment', 'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_Employees_IsEmployeeInDepartment;
GO

CREATE PROCEDURE dbo.sp_Employees_IsEmployeeInDepartment
    @EmployeeId INT,
    @DepartmentId INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT CASE
        WHEN EXISTS (
            SELECT 1
            FROM dbo.Employees
            WHERE Id = @EmployeeId
              AND DepartmentId = @DepartmentId
        ) THEN 1 ELSE 0
    END AS IsExists;
END;
GO

-- =====================================================
-- 14. sp_Employees_GetEmployeeWithRolesByEmail
-- Description: Get employee details with all assigned roles by email
-- Parameters: @Email NVARCHAR(255)
-- Returns: 
--   First result set: Employee details
--   Second result set: List of assigned roles
-- =====================================================
IF OBJECT_ID('dbo.sp_Employees_GetEmployeeWithRolesByEmail', 'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_Employees_GetEmployeeWithRolesByEmail;
GO

CREATE PROCEDURE dbo.sp_Employees_GetEmployeeWithRolesByEmail
    @Email NVARCHAR(255)
AS
BEGIN
    SET NOCOUNT ON;

    -- 1. Employee
    SELECT 
        e.Id,
        e.EmployeeCode,
        e.FullName,
        e.Email,
        e.Phone,
        e.DateOfBirth,
        e.Gender,
        e.Address,
        e.DepartmentId,
        e.Position,
        e.ManagerId,
        e.HireDate,
        e.ResignDate,
        e.Status,
        e.PasswordHash,
        e.RefreshToken,
        e.RefreshTokenExpiryTime,
        e.CreatedAt,
        e.UpdatedAt
    FROM dbo.Employees e
        WHERE e.Email = @Email
            AND e.IsActive = 1;

    -- 2. Roles của Employee
    SELECT 
        r.Id,
        r.Name,
        r.Description,
        r.IsSystem
    FROM dbo.Roles r
    INNER JOIN dbo.UserRoles ur ON r.Id = ur.RoleId
    INNER JOIN dbo.Employees e ON e.Id = ur.UserId
        WHERE e.Email = @Email
            AND e.IsActive = 1;
END;
GO

-- =====================================================
-- 15. sp_Employees_GetEmployeesWithDepartmentPaged
-- Description: Get paged employees with department info
-- Parameters:
--   @PageNumber INT (default 1)
--   @PageSize INT (default 20)
-- Returns:
--   First result set: paged employees with department
--   Second result set: total record count
-- =====================================================

CREATE PROCEDURE dbo.sp_Employees_GetEmployeesWithDepartmentPaged
    @PageNumber INT = 1,
    @PageSize INT = 20
AS
BEGIN
    SET NOCOUNT ON;

    IF @PageNumber < 1 SET @PageNumber = 1;
    IF @PageSize < 1 SET @PageSize = 20;

    DECLARE @Offset INT = (@PageNumber - 1) * @PageSize;

    -- 1. Employees
    SELECT
        e.Id,
        e.EmployeeCode,
        e.FullName,
        e.Email,
        e.Phone,
        e.DateOfBirth,
        e.Gender,
        e.Address,
        e.DepartmentId,
        e.Position,
        e.ManagerId,
        e.HireDate,
        e.ResignDate,
        e.IsActive,
        e.Status,
        e.CreatedAt,
        e.UpdatedAt
    FROM dbo.Employees e
    WHERE e.IsActive = 1
    ORDER BY e.FullName
    OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;

    -- 2. Departments (distinct)
    SELECT
        d.Id,
        d.Name
    FROM dbo.Departments d;

    -- 3. Total count
    SELECT COUNT(1) AS TotalCount
    FROM dbo.Employees
    WHERE IsActive = 1;
END;
GO

-- =====================================================
-- 16. sp_Employees_GetEmployeesByDepartmentPaged
-- Description: Get paged employees filtered by department with joined department info
-- Parameters:
--   @DepartmentId INT
--   @PageNumber INT (default 1)
--   @PageSize INT (default 20)
-- Returns:
--   First result set: paged employees with joined department info
--   Second result set: total record count of filtered data
-- =====================================================


CREATE PROCEDURE dbo.sp_Employees_GetEmployeesByDepartmentPaged
    @DepartmentId INT,
    @PageNumber INT = 1,
    @PageSize INT = 20
AS
BEGIN
    SET NOCOUNT ON;

    IF @PageNumber < 1 SET @PageNumber = 1;
    IF @PageSize < 1 SET @PageSize = 20;

    DECLARE @Offset INT = (@PageNumber - 1) * @PageSize;

    -- 1. Employees with joined Department (using DeptId as split point for Dapper multi-mapping)
    SELECT
        e.Id,
        e.EmployeeCode,
        e.FullName,
        e.Email,
        e.Phone,
        e.DateOfBirth,
        e.Gender,
        e.Address,
        e.DepartmentId,
        e.Position,
        e.ManagerId,
        e.HireDate,
        e.ResignDate,
        e.IsActive,
        e.Status,
        e.CreatedAt,
        e.UpdatedAt,
        d.Id AS DeptId,
        d.Name
    FROM dbo.Employees e
    INNER JOIN dbo.Departments d ON e.DepartmentId = d.Id
    WHERE e.DepartmentId = @DepartmentId
        AND e.IsActive = 1
    ORDER BY e.FullName
    OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;

    -- 2. Total count
    SELECT COUNT(1) AS TotalCount
    FROM dbo.Employees
    WHERE DepartmentId = @DepartmentId
        AND IsActive = 1;
END;
GO

-- =====================================================
-- 17. sp_Employees_GetEmployeesByRoleName
-- Description: Get active employees by role name
-- Parameters:
--   @RoleName NVARCHAR(100)
-- Returns:
--   List of employees with department info
-- =====================================================
IF OBJECT_ID('dbo.sp_Employees_GetEmployeesByRoleName', 'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_Employees_GetEmployeesByRoleName;
GO

CREATE PROCEDURE dbo.sp_Employees_GetEmployeesByRoleName
    @RoleName NVARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;

    -- 1. Employees
    SELECT
        e.Id,
        e.EmployeeCode,
        e.FullName,
        e.DepartmentId
    FROM dbo.Employees e
    INNER JOIN dbo.UserRoles ur ON ur.UserId = e.Id
    INNER JOIN dbo.Roles r ON r.Id = ur.RoleId
    WHERE e.IsActive = 1
      AND r.Name = @RoleName
    ORDER BY e.FullName;

    -- 2. Departments
    SELECT DISTINCT
        d.Id,
        d.Name
    FROM dbo.Departments d
    INNER JOIN dbo.Employees e ON e.DepartmentId = d.Id
    INNER JOIN dbo.UserRoles ur ON ur.UserId = e.Id
    INNER JOIN dbo.Roles r ON r.Id = ur.RoleId
    WHERE e.IsActive = 1
      AND r.Name = @RoleName;
END;
GO

-- =====================================================
-- 18. sp_Employees_GetEmployeeIdByEmployeeCode
-- Description: Get employee ID by employee code
-- Parameters: @EmployeeCode NVARCHAR(50)
-- Returns: EmployeeId
-- =====================================================
CREATE PROCEDURE dbo.sp_Employees_GetEmployeeIdByEmployeeCode
    @EmployeeCode NVARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT Id
    FROM dbo.Employees
    WHERE EmployeeCode = @EmployeeCode;
END;
GO



