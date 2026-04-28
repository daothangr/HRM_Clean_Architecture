-- =====================================================
-- DATABASE: HRM_System
-- =====================================================
-- DROP DATABASE IF EXISTS HRM_System;
CREATE DATABASE HRM_System;
GO

USE HRM_System;
GO

-- =====================================================
-- TABLE: Departments (Phòng ban)
-- =====================================================
CREATE TABLE Departments (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Code NVARCHAR(50) UNIQUE NOT NULL,
    Name NVARCHAR(255) NOT NULL,
    ParentDepartmentId INT NULL,
    DepartmentHeadId INT NULL,
    Status TINYINT DEFAULT 1, -- 1: Active, 0: Inactive
    CreatedAt DATETIME2 DEFAULT GETDATE(),
    UpdatedAt DATETIME2 NULL,
    CONSTRAINT FK_Department_Parent FOREIGN KEY (ParentDepartmentId) REFERENCES Departments(Id)
);
GO

-- =====================================================
-- TABLE: Employees (Nhân viên)
-- =====================================================
CREATE TABLE Employees (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    EmployeeCode NVARCHAR(50) UNIQUE NOT NULL,
    FullName NVARCHAR(255) NOT NULL,
    Email NVARCHAR(255) UNIQUE NOT NULL,
    Phone NVARCHAR(20) NULL,
    DateOfBirth DATETIME2 NULL,
    Gender TINYINT NULL, -- 1: Male, 2: Female, 3: Other
    Address NVARCHAR(500) NULL,
    DepartmentId INT NOT NULL,
    Position NVARCHAR(100) NULL,
    ManagerId INT NULL, -- Quản lý trực tiếp
    HireDate DATETIME2 NULL,
    ResignDate DATETIME2 NULL,
    IsActive BIT NOT NULL DEFAULT 1,
    Status TINYINT DEFAULT 1, -- 1: Active, 2: Probation, 3: Resigned
    PasswordHash NVARCHAR(255) NOT NULL,
    RefreshToken NVARCHAR(255) NULL,
    RefreshTokenExpiryTime DATETIME2 NULL,
    CreatedAt DATETIME2 DEFAULT GETDATE(),
    UpdatedAt DATETIME2 NULL,
    CONSTRAINT FK_Employee_Department FOREIGN KEY (DepartmentId) REFERENCES Departments(Id),
    CONSTRAINT FK_Employee_Manager FOREIGN KEY (ManagerId) REFERENCES Employees(Id)
);
GO

-- Index for performance
CREATE INDEX IX_Employees_DepartmentId ON Employees(DepartmentId);
CREATE INDEX IX_Employees_ManagerId ON Employees(ManagerId);
CREATE INDEX IX_Employees_Status ON Employees(Status);
CREATE INDEX IX_Employees_Email ON Employees(Email);
GO

-- =====================================================
-- TABLE: Roles (Vai trò)
-- =====================================================
CREATE TABLE Roles (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Name NVARCHAR(100) NOT NULL, -- Admin, Director, Manager, Employee
    Description NVARCHAR(500) NULL,
    IsSystem BIT DEFAULT 0
);
GO

-- =====================================================
-- TABLE: Permissions (Quyền)
-- =====================================================
CREATE TABLE Permissions (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Name NVARCHAR(100) NOT NULL, -- view_employees, approve_leave, etc.
    Resource NVARCHAR(100) NOT NULL,
    Action NVARCHAR(50) NOT NULL,
    Description NVARCHAR(500) NULL
);
GO

-- =====================================================
-- TABLE: RolePermissions (Phân quyền)
-- =====================================================
CREATE TABLE RolePermissions (
    RoleId INT NOT NULL,
    PermissionId INT NOT NULL,
    PRIMARY KEY (RoleId, PermissionId),
    CONSTRAINT FK_RolePermissions_Role FOREIGN KEY (RoleId) REFERENCES Roles(Id),
    CONSTRAINT FK_RolePermissions_Permission FOREIGN KEY (PermissionId) REFERENCES Permissions(Id)
);
GO

-- =====================================================
-- TABLE: UserRoles (Phân quyền người dùng)
-- =====================================================
CREATE TABLE UserRoles (
    UserId INT NOT NULL,
    RoleId INT NOT NULL,
    PRIMARY KEY (UserId, RoleId),
    CONSTRAINT FK_UserRoles_User FOREIGN KEY (UserId) REFERENCES Employees(Id),
    CONSTRAINT FK_UserRoles_Role FOREIGN KEY (RoleId) REFERENCES Roles(Id)
);
GO

-- =====================================================
-- TABLE: LeaveRequests (Đơn nghỉ phép)
-- =====================================================
CREATE TABLE LeaveRequests (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    EmployeeId INT NOT NULL,
    LeaveType TINYINT NOT NULL, 
    StartDate DATETIME2 NOT NULL,
    EndDate DATETIME2 NOT NULL,
    IsFullDay BIT NOT NULL DEFAULT 1,
    StartTime TIME NULL,
    EndTime TIME NULL,
    TotalHours DECIMAL(5,2) NULL,
    TotalDays DECIMAL(4,1) NOT NULL,
    Reason NVARCHAR(1000) NULL,
    Status TINYINT NOT NULL DEFAULT 1, -- enum LeaveRequestStatus
    CurrentApprovalLevel INT NOT NULL DEFAULT 1,
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    UpdatedAt DATETIME2 NULL,
    CONSTRAINT FK_LeaveRequest_Employee 
        FOREIGN KEY (EmployeeId) REFERENCES Employees(Id),
    CONSTRAINT CK_LeaveRequest_Time CHECK (
        (IsFullDay = 1 AND StartTime IS NULL AND EndTime IS NULL AND TotalHours IS NULL)
        OR
        (IsFullDay = 0 AND StartTime IS NOT NULL AND EndTime IS NOT NULL)
    )
);
GO
CREATE INDEX IX_LeaveRequests_EmployeeId ON LeaveRequests(EmployeeId);
CREATE INDEX IX_LeaveRequests_Status ON LeaveRequests(Status);
CREATE INDEX IX_LeaveRequests_StartDate_EndDate ON LeaveRequests(StartDate, EndDate);
GO

-- =====================================================
-- TABLE: ApprovalWorkflows (Luồng duyệt)
-- =====================================================
CREATE TABLE ApprovalWorkflows (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    RequestType NVARCHAR(50) NOT NULL, -- leave, overtime, business_trip
    DepartmentId INT NULL,
    Level INT NOT NULL,
    ApproverRole TINYINT NOT NULL, -- 1: Manager, 2: DepartmentHead, 3: Director, 4: HR
    MaxDaysAllowed INT NULL,
    IsActive BIT DEFAULT 1,
    CreatedAt DATETIME2 DEFAULT GETDATE()
);
GO

-- =====================================================
-- TABLE: ApprovalLogs (Lịch sử duyệt)
-- =====================================================
CREATE TABLE ApprovalLogs (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    RequestId INT NOT NULL,
    RequestType NVARCHAR(50) NOT NULL,
    ApproverId INT NOT NULL,
    Action TINYINT NOT NULL, -- 1: Approved, 2: Rejected, 3: Forwarded
    Comment NVARCHAR(500) NULL,
    Level INT NOT NULL,
    CreatedAt DATETIME2 DEFAULT GETDATE(),
    CONSTRAINT FK_ApprovalLog_Approver FOREIGN KEY (ApproverId) REFERENCES Employees(Id)
);
GO
CREATE INDEX IX_ApprovalLogs_RequestId ON ApprovalLogs(RequestId);
GO

-- =====================================================
-- TABLE: LeaveBalances (Số dư ngày phép)
-- =====================================================
CREATE TABLE LeaveBalances (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    EmployeeId INT NOT NULL,
    Year INT NOT NULL,
    AnnualLeave DECIMAL(5,1) DEFAULT 12,
    SickLeave DECIMAL(5,1) DEFAULT 3,
    UsedAnnual DECIMAL(5,1) DEFAULT 0,
    UsedSick DECIMAL(5,1) DEFAULT 0,
    CONSTRAINT FK_LeaveBalance_Employee FOREIGN KEY (EmployeeId) REFERENCES Employees(Id),
    CONSTRAINT UQ_LeaveBalance_Employee_Year UNIQUE (EmployeeId, Year)
);
GO

-- =====================================================
-- STORED PROCEDURE: GetLeaveBalanceByEmployeeAndYear
-- =====================================================
CREATE PROCEDURE sp_LeaveBalances_GetByEmployeeAndYear
    @EmployeeId INT,
    @Year INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT lb.*
    FROM LeaveBalances lb
    WHERE lb.EmployeeId = @EmployeeId
      AND lb.Year = @Year;
END
GO

-- =====================================================
-- TABLE: Attendance (Chấm công)
-- =====================================================
CREATE TABLE Attendances (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    EmployeeId INT NOT NULL,
    Date DATE NOT NULL,
    CheckInTime TIME NULL,
    CheckOutTime TIME NULL,
    WorkHours DECIMAL(5,2) NULL,
    OvertimeHours DECIMAL(5,2) DEFAULT 0,
    Status TINYINT DEFAULT 1, -- 1: Present, 2: Absent, 3: Late, 4: EarlyLeave
    CONSTRAINT FK_Attendance_Employee FOREIGN KEY (EmployeeId) REFERENCES Employees(Id),
    CONSTRAINT UQ_Attendance_Employee_Date UNIQUE (EmployeeId, Date)
);
GO

-- =====================================================
-- TABLE: ActivityLogs (Nhật ký hoạt động)
-- =====================================================
CREATE TABLE ActivityLogs (
    Id BIGINT IDENTITY(1,1) PRIMARY KEY,
    UserId INT NULL,
    Action NVARCHAR(100) NOT NULL,
    EntityType NVARCHAR(100) NULL,
    EntityId NVARCHAR(100) NULL,
    OldValues NVARCHAR(MAX) NULL,
    NewValues NVARCHAR(MAX) NULL,
    IpAddress NVARCHAR(45) NULL,
    UserAgent NVARCHAR(500) NULL,
    CreatedAt DATETIME2 DEFAULT GETDATE(),
    CONSTRAINT FK_ActivityLog_User FOREIGN KEY (UserId) REFERENCES Employees(Id)
);
GO
-- Index for ActivityLogs
CREATE INDEX IX_ActivityLogs_UserId ON ActivityLogs(UserId);
CREATE INDEX IX_ActivityLogs_CreatedAt ON ActivityLogs(CreatedAt);
GO

-- =====================================================
-- STORED PROCEDURE: GetLeaveRequestsByApprover
-- =====================================================
CREATE PROCEDURE sp_GetLeaveRequestsByApprover
    @ApproverId INT,
    @Status TINYINT = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @DepartmentId INT;
    DECLARE @RoleId INT;
    
    SELECT @DepartmentId = DepartmentId FROM Employees WHERE Id = @ApproverId;
    SELECT @RoleId = RoleId FROM UserRoles WHERE UserId = @ApproverId;
    
    IF @RoleId IN (1, 2) -- Admin or Director
    BEGIN
        SELECT lr.*, e.FullName, e.EmployeeCode, d.Name as DepartmentName
        FROM LeaveRequests lr
        INNER JOIN Employees e ON lr.EmployeeId = e.Id
        INNER JOIN Departments d ON e.DepartmentId = d.Id
        WHERE (@Status IS NULL OR lr.Status = @Status)
        ORDER BY lr.CreatedAt DESC;
    END
    ELSE IF @RoleId = 3 -- Manager
    BEGIN
        SELECT lr.*, e.FullName, e.EmployeeCode, d.Name as DepartmentName
        FROM LeaveRequests lr
        INNER JOIN Employees e ON lr.EmployeeId = e.Id
        INNER JOIN Departments d ON e.DepartmentId = d.Id
        WHERE e.DepartmentId = @DepartmentId 
          AND (@Status IS NULL OR lr.Status = @Status)
        ORDER BY lr.CreatedAt DESC;
    END
END
GO
