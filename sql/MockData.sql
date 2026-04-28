USE HRM_System;
GO

-- =====================================================
-- MOCK DATA GENERATION SCRIPT
-- =====================================================
-- This script generates:
-- 1. 1000 mock employees
-- 2. 10 million mock attendance records
-- =====================================================

SET NOCOUNT ON;
GO

-- =====================================================
-- STEP 1: Create helper table for bulk data generation
-- =====================================================
IF OBJECT_ID('tempdb..#Numbers', 'U') IS NOT NULL
    DROP TABLE #Numbers;

-- Generate numbers 1 to 10,000 (for efficient generation)
WITH Numbers AS (
    SELECT TOP 10000 ROW_NUMBER() OVER (ORDER BY (SELECT NULL)) AS N
    FROM sys.objects a
    CROSS JOIN sys.objects b
    CROSS JOIN sys.objects c
)
SELECT N INTO #Numbers FROM Numbers;

CREATE CLUSTERED INDEX IX_Numbers ON #Numbers(N);
GO

-- =====================================================
-- STEP 2: Insert 1000 Mock Employees
-- =====================================================
PRINT 'Creating 1000 mock employees...';

IF EXISTS (SELECT 1 FROM Employees WHERE EmployeeCode LIKE 'EMP%' OR Email LIKE 'emp%@company.com')
BEGIN
    PRINT 'Existing mock employees detected. Skipping employee generation to avoid duplicate keys.';
END
ELSE
BEGIN

DECLARE @DepartmentIds TABLE (
    RowNumber INT IDENTITY(1,1) PRIMARY KEY,
    DepartmentId INT NOT NULL
);

DECLARE @DepartmentSeed TABLE (
    Code NVARCHAR(50) NOT NULL,
    Name NVARCHAR(255) NOT NULL
);

INSERT INTO @DepartmentSeed (Code, Name)
VALUES
    ('IT', 'Information Technology'),
    ('HR', 'Human Resources'),
    ('FIN', 'Finance'),
    ('OPS', 'Operations'),
    ('SAL', 'Sales'),
    ('MKT', 'Marketing'),
    ('CUS', 'Customer Support'),
    ('PRD', 'Product'),
    ('QA', 'Quality Assurance'),
    ('ADM', 'Administration');

DECLARE @SeedCode NVARCHAR(50);
DECLARE @SeedName NVARCHAR(255);

DECLARE DepartmentCursor CURSOR LOCAL FAST_FORWARD FOR
SELECT Code, Name
FROM @DepartmentSeed;

OPEN DepartmentCursor;
FETCH NEXT FROM DepartmentCursor INTO @SeedCode, @SeedName;

WHILE @@FETCH_STATUS = 0
BEGIN
    IF NOT EXISTS (SELECT 1 FROM Departments WHERE Code = @SeedCode)
    BEGIN
        INSERT INTO Departments (Code, Name, Status)
        VALUES (@SeedCode, @SeedName, 1);
    END

    FETCH NEXT FROM DepartmentCursor INTO @SeedCode, @SeedName;
END;

CLOSE DepartmentCursor;
DEALLOCATE DepartmentCursor;

INSERT INTO @DepartmentIds (DepartmentId)
SELECT TOP 10 Id
FROM Departments
WHERE Code IN ('IT', 'HR', 'FIN', 'OPS', 'SAL', 'MKT', 'CUS', 'PRD', 'QA', 'ADM')
ORDER BY Id;

IF (SELECT COUNT(*) FROM @DepartmentIds) < 10
BEGIN
    THROW 50001, 'Unable to seed 10 departments for mock data generation.', 1;
END

DECLARE @Counter INT = 1;
DECLARE @EmployeeCount INT = 1000;

WHILE @Counter <= @EmployeeCount
BEGIN
    DECLARE @EmployeeCode NVARCHAR(50) = 'EMP' + RIGHT('00000' + CAST(@Counter AS NVARCHAR(5)), 5);
    DECLARE @FullName NVARCHAR(255) = 'Employee ' + CAST(@Counter AS NVARCHAR(5));
    DECLARE @Email NVARCHAR(255) = 'emp' + CAST(@Counter AS NVARCHAR(5)) + '@company.com';
    DECLARE @Phone NVARCHAR(20) = '090' + RIGHT('0000000' + CAST(@Counter AS NVARCHAR(7)), 7);
    DECLARE @DateOfBirth DATETIME2 = DATEADD(YEAR, -30 + (@Counter % 20), DATEADD(DAY, -(@Counter % 365), GETDATE()));
    DECLARE @Gender TINYINT = 1 + (@Counter % 3); -- 1, 2, or 3
    DECLARE @ManagerId INT = NULL;
    DECLARE @EmployeeDepartmentId INT = (
        SELECT DepartmentId
        FROM @DepartmentIds
        WHERE RowNumber = ((@Counter - 1) % 10) + 1
    );

    -- Set manager for some employees
    IF @Counter > 10
    BEGIN
        SET @ManagerId = 1 + ((@Counter - 11) % 10);
    END

    INSERT INTO Employees (
        EmployeeCode, FullName, Email, Phone, DateOfBirth, Gender,
        DepartmentId, Position, ManagerId, HireDate, IsActive, Status,
        PasswordHash, RefreshToken, RefreshTokenExpiryTime
    )
    VALUES (
        @EmployeeCode, @FullName, @Email, @Phone, @DateOfBirth, @Gender,
        @EmployeeDepartmentId, 'Position ' + CAST(@Counter % 5 AS NVARCHAR(5)), @ManagerId, 
        DATEADD(YEAR, -(@Counter % 5), GETDATE()), 1, 1,
        'MOCK_PASSWORD_HASH_' + CAST(@Counter AS NVARCHAR(5)), NULL, NULL
    );

    SET @Counter = @Counter + 1;

    -- Print progress every 100 employees
    IF @Counter % 100 = 0
        PRINT 'Created ' + CAST(@Counter - 1 AS NVARCHAR(10)) + ' employees...';
END;

PRINT '✓ Successfully created 1000 mock employees.';
END
GO

-- =====================================================
-- STEP 2B: Assign Roles to Mock Employees
-- =====================================================
PRINT '';
PRINT 'Assigning roles to mock employees...';

DECLARE @EmployeeRoleId INT;
DECLARE @AdminRoleId INT;
DECLARE @DirectorRoleId INT;
DECLARE @ManagerRoleId INT;

IF NOT EXISTS (SELECT 1 FROM Roles WHERE Name = 'Employee')
    INSERT INTO Roles (Name, Description, IsSystem) VALUES ('Employee', 'Employee', 1);
IF NOT EXISTS (SELECT 1 FROM Roles WHERE Name = 'Admin')
    INSERT INTO Roles (Name, Description, IsSystem) VALUES ('Admin', 'Admin', 1);
IF NOT EXISTS (SELECT 1 FROM Roles WHERE Name = 'Director')
    INSERT INTO Roles (Name, Description, IsSystem) VALUES ('Director', 'Director', 1);
IF NOT EXISTS (SELECT 1 FROM Roles WHERE Name = 'Manager')
    INSERT INTO Roles (Name, Description, IsSystem) VALUES ('Manager', 'Manager', 1);

SELECT @EmployeeRoleId = Id FROM Roles WHERE Name = 'Employee';
SELECT @AdminRoleId = Id FROM Roles WHERE Name = 'Admin';
SELECT @DirectorRoleId = Id FROM Roles WHERE Name = 'Director';
SELECT @ManagerRoleId = Id FROM Roles WHERE Name = 'Manager';

IF @EmployeeRoleId IS NULL OR @AdminRoleId IS NULL OR @DirectorRoleId IS NULL OR @ManagerRoleId IS NULL
BEGIN
    THROW 50002, 'Required roles were not found. Please seed roles before running mock data.', 1;
END

IF EXISTS (SELECT 1 FROM Employees WHERE EmployeeCode LIKE 'EMP%')
BEGIN
    DELETE ur
    FROM UserRoles ur
    INNER JOIN Employees e ON e.Id = ur.UserId
    WHERE e.EmployeeCode LIKE 'EMP%';

    ;WITH MockEmployees AS (
        SELECT
            e.Id,
            ROW_NUMBER() OVER (ORDER BY e.Id) AS RowNumber,
            COUNT(*) OVER () AS TotalCount
        FROM Employees e
        WHERE e.EmployeeCode LIKE 'EMP%'
    )
    INSERT INTO UserRoles (UserId, RoleId)
    SELECT
        me.Id,
        CASE
            WHEN me.RowNumber <= CAST(me.TotalCount * 0.80 AS INT) THEN @EmployeeRoleId
            WHEN me.RowNumber <= CAST(me.TotalCount * 0.90 AS INT) THEN @AdminRoleId
            WHEN me.RowNumber <= CAST(me.TotalCount * 0.95 AS INT) THEN @DirectorRoleId
            ELSE @ManagerRoleId
        END AS RoleId
    FROM MockEmployees me
    WHERE NOT EXISTS (
        SELECT 1
        FROM UserRoles ur
        WHERE ur.UserId = me.Id
          AND ur.RoleId = CASE
              WHEN me.RowNumber <= CAST(me.TotalCount * 0.80 AS INT) THEN @EmployeeRoleId
              WHEN me.RowNumber <= CAST(me.TotalCount * 0.90 AS INT) THEN @AdminRoleId
              WHEN me.RowNumber <= CAST(me.TotalCount * 0.95 AS INT) THEN @DirectorRoleId
              ELSE @ManagerRoleId
          END
    );

    PRINT '✓ Successfully assigned roles to mock employees (80% Employee, 10% Admin, 5% Director, 5% Manager).';
END
ELSE
BEGIN
    PRINT 'No mock employees found. Skipping role assignment.';
END

-- =====================================================
-- STEP 3: Insert 10 Million Mock Attendance Records (Optimized)
-- =====================================================
PRINT '';
PRINT 'Creating 10 million mock attendance records...';
PRINT 'This may take 5-15 minutes depending on system performance...';

DECLARE @StartDate DATE = DATEADD(DAY, -9999, CAST(GETDATE() AS DATE)); -- 10,000 days back
DECLARE @BatchSize INT = 1000; -- 1,000 days per batch
DECLARE @TotalBatches INT = 10; -- 10 batches × (1,000 employees × 1,000 days) = 10M records
DECLARE @CurrentBatch INT = 0;

-- Create extended numbers table for efficient bulk generation
IF OBJECT_ID('tempdb..#ExtendedNumbers', 'U') IS NOT NULL
    DROP TABLE #ExtendedNumbers;

-- Generate 500K sequential numbers for one batch
WITH NumbersCTE AS (
    SELECT TOP 500000 ROW_NUMBER() OVER (ORDER BY (SELECT NULL)) AS N
    FROM sys.objects a
    CROSS JOIN sys.objects b
    CROSS JOIN sys.objects c
    CROSS JOIN sys.objects d
)
SELECT N INTO #ExtendedNumbers FROM NumbersCTE;

CREATE CLUSTERED INDEX IX_ExtendedNumbers ON #ExtendedNumbers(N);

IF OBJECT_ID('tempdb..#AttendanceDates', 'U') IS NOT NULL
    DROP TABLE #AttendanceDates;

SELECT
    DATEADD(DAY, n.N - 1, @StartDate) AS AttendanceDate,
    n.N AS DayNumber
INTO #AttendanceDates
FROM #ExtendedNumbers n
WHERE n.N <= 10000;

CREATE CLUSTERED INDEX IX_AttendanceDates ON #AttendanceDates(DayNumber);

-- Insert batches
WHILE @CurrentBatch < @TotalBatches
BEGIN
    BEGIN TRY
        BEGIN TRANSACTION;

        DECLARE @DateStart INT = (@CurrentBatch * @BatchSize) + 1;
        DECLARE @DateEnd INT = (@CurrentBatch + 1) * @BatchSize;

        INSERT INTO Attendances (EmployeeId, Date, CheckInTime, CheckOutTime, WorkHours, OvertimeHours, Status)
        SELECT
            e.Id AS EmployeeId,
            d.AttendanceDate AS [Date],
            CAST(DATEADD(MINUTE, 480 + ABS(CHECKSUM(e.Id, d.DayNumber)) % 120, 0) AS TIME) AS CheckInTime,
            CAST(DATEADD(MINUTE, 1020 + ABS(CHECKSUM(e.Id, d.DayNumber, 1)) % 120, 0) AS TIME) AS CheckOutTime,
            CAST(7.50 + (ABS(CHECKSUM(e.Id, d.DayNumber, 2)) % 100) / CAST(50 AS DECIMAL(5,2)) AS DECIMAL(5,2)) AS WorkHours,
            CAST((ABS(CHECKSUM(e.Id, d.DayNumber, 3)) % 20) / CAST(10 AS DECIMAL(5,2)) AS DECIMAL(5,2)) AS OvertimeHours,
            1 + (ABS(CHECKSUM(e.Id, d.DayNumber, 4)) % 4) AS Status
        FROM Employees e
        CROSS JOIN #AttendanceDates d
        LEFT JOIN Attendances a ON a.EmployeeId = e.Id AND a.[Date] = d.AttendanceDate
        WHERE e.EmployeeCode LIKE 'EMP%'
          AND d.DayNumber BETWEEN @DateStart AND @DateEnd
          AND a.Id IS NULL;

        COMMIT TRANSACTION;

        SET @CurrentBatch = @CurrentBatch + 1;
        DECLARE @RecordsInserted BIGINT = @CurrentBatch * @BatchSize * 1000;
        DECLARE @Percentage DECIMAL(5,2) = (CAST(@RecordsInserted AS DECIMAL) / 10000000.0) * 100;

        PRINT 'Batch ' + CAST(@CurrentBatch AS NVARCHAR(3)) + '/' + CAST(@TotalBatches AS NVARCHAR(3)) + 
              ' completed - Inserted ' + CAST(@RecordsInserted AS NVARCHAR(12)) + ' records (' + 
              CAST(@Percentage AS NVARCHAR(6)) + '%)';

    END TRY
    BEGIN CATCH
        PRINT 'ERROR in batch ' + CAST(@CurrentBatch AS NVARCHAR(3)) + ': ' + ERROR_MESSAGE();
        ROLLBACK TRANSACTION;
        SET @CurrentBatch = @TotalBatches; -- Exit loop
    END CATCH
END;

DROP TABLE #ExtendedNumbers;
DROP TABLE #AttendanceDates;

PRINT '';
PRINT '✓ Successfully created 10,000,000 mock attendance records.';
GO

-- =====================================================
-- STEP 4: Cleanup and Verification
-- =====================================================
IF OBJECT_ID('tempdb..#Numbers', 'U') IS NOT NULL
    DROP TABLE #Numbers;
GO

-- Verify data
PRINT '';
PRINT '=== DATA VERIFICATION ===';

SELECT 'Employees' AS [Table], COUNT(*) AS [Record Count] FROM Employees
UNION ALL
SELECT 'Attendances', COUNT(*) FROM Attendances
UNION ALL
SELECT 'Departments', COUNT(*) FROM Departments WHERE Code IN ('IT', 'HR', 'FIN', 'OPS', 'SAL', 'MKT', 'CUS', 'PRD', 'QA', 'ADM');

PRINT '';
PRINT 'Sample Department Data:';
SELECT TOP 10 Id, Code, Name, Status FROM Departments WHERE Code IN ('IT', 'HR', 'FIN', 'OPS', 'SAL', 'MKT', 'CUS', 'PRD', 'QA', 'ADM') ORDER BY Id;

PRINT '';
PRINT 'Sample Employee Data:';
SELECT TOP 5 Id, EmployeeCode, FullName, Email, DepartmentId FROM Employees ORDER BY Id;

PRINT '';
PRINT 'Sample Attendance Data:';
SELECT TOP 5 Id, EmployeeId, Date, CheckInTime, CheckOutTime, WorkHours, Status FROM Attendances ORDER BY Id;

PRINT '';
PRINT '=== MOCK DATA GENERATION COMPLETED SUCCESSFULLY ===';
