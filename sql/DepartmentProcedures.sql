USE HRM_System;
GO

-- =====================================================
-- DEPARTMENT SERVICE STORED PROCEDURES
-- =====================================================
-- This file contains all stored procedures for Department
-- operations following the project's naming convention:
-- sp_[EntityName]_[Action]

-- =====================================================
-- 1. sp_Departments_GetDepartments
-- Description: Get all departments, optionally including inactive ones
-- Parameters: @IncludeInactive BIT (1 = include inactive, 0 = only active)
-- Returns: List of departments ordered by name
-- =====================================================
IF OBJECT_ID('dbo.sp_Departments_GetDepartments', 'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_Departments_GetDepartments;
GO

CREATE PROCEDURE dbo.sp_Departments_GetDepartments
    @IncludeInactive BIT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT 
        Id,
        Code,
        Name,
        Status,
        DepartmentHeadId,
        ParentDepartmentId,
        CreatedAt,
        UpdatedAt
    FROM dbo.Departments
    WHERE (@IncludeInactive = 1 OR Status = 1)
    ORDER BY Name;
END;
GO

-- =====================================================
-- 2. sp_Departments_CheckDepartmentCodeExists
-- Description: Check if a department code already exists
-- Parameters: 
--   @Code NVARCHAR(50)
--   @ExcludeDepartmentId INT (optional, for update scenarios)
-- Returns: INT (1 if exists, 0 if not exists)
-- =====================================================
IF OBJECT_ID('dbo.sp_Departments_CheckDepartmentCodeExists', 'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_Departments_CheckDepartmentCodeExists;
GO

CREATE PROCEDURE dbo.sp_Departments_CheckDepartmentCodeExists
    @Code NVARCHAR(50),
    @ExcludeDepartmentId INT = NULL
AS
BEGIN
    SET NOCOUNT ON;

    SELECT CASE 
        WHEN EXISTS (
            SELECT 1
            FROM dbo.Departments
            WHERE Code = LTRIM(RTRIM(@Code))
              AND (@ExcludeDepartmentId IS NULL OR Id <> @ExcludeDepartmentId)
        )
        THEN 1 ELSE 0
    END AS IsExists;
END;
GO

-- =====================================================
-- 3. sp_Departments_CheckDepartmentExists
-- Description: Check if a department exists by ID
-- Parameters: @DepartmentId INT
-- Returns: INT (1 if exists, 0 if not exists)
-- =====================================================
IF OBJECT_ID('dbo.sp_Departments_CheckDepartmentExists', 'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_Departments_CheckDepartmentExists;
GO

CREATE PROCEDURE dbo.sp_Departments_CheckDepartmentExists
    @DepartmentId INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT CASE 
        WHEN EXISTS (
            SELECT 1
            FROM dbo.Departments
            WHERE Id = @DepartmentId
        )
        THEN 1 ELSE 0
    END AS IsExists;
END;
GO

-- =====================================================
-- 4. sp_Departments_CheckDepartmentHasEmployees
-- Description: Check if a department has any employees
-- Parameters: @DepartmentId INT
-- Returns: INT (1 if has employees, 0 if empty)
-- =====================================================
IF OBJECT_ID('dbo.sp_Departments_CheckDepartmentHasEmployees', 'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_Departments_CheckDepartmentHasEmployees;
GO

CREATE PROCEDURE dbo.sp_Departments_CheckDepartmentHasEmployees
    @DepartmentId INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT CASE 
        WHEN EXISTS (
            SELECT 1
            FROM dbo.Employees
            WHERE DepartmentId = @DepartmentId
        )
        THEN 1 ELSE 0
    END AS HasEmployees;
END;
GO

-- =====================================================
-- 5. sp_Departments_GetDepartmentById
-- Description: Get a specific department by ID
-- Parameters: @DepartmentId INT
-- Returns: Department details
-- =====================================================
IF OBJECT_ID('dbo.sp_Departments_GetDepartmentById', 'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_Departments_GetDepartmentById;
GO

CREATE PROCEDURE dbo.sp_Departments_GetDepartmentById
    @DepartmentId INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT 
        Id,
        Code,
        Name,
        Status,
        DepartmentHeadId,
        ParentDepartmentId,
        CreatedAt,
        UpdatedAt
    FROM dbo.Departments
    WHERE Id = @DepartmentId;
END;
GO

-- =====================================================
-- 6. sp_Departments_AddDepartment
-- Description: Create a new department
-- Parameters: 
--   @Code NVARCHAR(50)
--   @Name NVARCHAR(200)
--   @Status INT
-- Returns: NewId (SCOPE_IDENTITY)
-- =====================================================
IF OBJECT_ID('dbo.sp_Departments_AddDepartment', 'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_Departments_AddDepartment;
GO

CREATE PROCEDURE dbo.sp_Departments_AddDepartment
    @Code NVARCHAR(50),
    @Name NVARCHAR(200),
    @Status INT
AS
BEGIN
    SET NOCOUNT ON;

    INSERT INTO dbo.Departments
    (
        Code,
        Name,
        Status,
        CreatedAt
    )
    VALUES
    (
        LTRIM(RTRIM(@Code)),
        @Name,
        @Status,
        GETUTCDATE()
    );

    SELECT SCOPE_IDENTITY() AS NewId;
END;
GO

-- =====================================================
-- 7. sp_Departments_UpdateDepartment
-- Description: Update an existing department
-- Parameters: 
--   @Id INT
--   @Code NVARCHAR(50)
--   @Name NVARCHAR(200)
--   @Status INT
-- Returns: None
-- =====================================================
IF OBJECT_ID('dbo.sp_Departments_UpdateDepartment', 'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_Departments_UpdateDepartment;
GO

CREATE PROCEDURE dbo.sp_Departments_UpdateDepartment
    @Id INT,
    @Code NVARCHAR(50),
    @Name NVARCHAR(200),
    @Status INT
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE dbo.Departments
    SET 
        Code = LTRIM(RTRIM(@Code)),
        Name = @Name,
        Status = @Status,
        UpdatedAt = GETUTCDATE()
    WHERE Id = @Id;
END;
GO
