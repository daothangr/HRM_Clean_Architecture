using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HRM.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddLeaveHoursOvertimeAttendanceAdj : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<TimeOnly>(
                name: "EndTime",
                table: "LeaveRequests",
                type: "time",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsFullDay",
                table: "LeaveRequests",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<TimeOnly>(
                name: "StartTime",
                table: "LeaveRequests",
                type: "time",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "TotalHours",
                table: "LeaveRequests",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "AttendanceAdjustmentRequests",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EmployeeId = table.Column<int>(type: "int", nullable: false),
                    WorkDate = table.Column<DateOnly>(type: "date", nullable: false),
                    RequestedCheckIn = table.Column<TimeOnly>(type: "time", nullable: true),
                    RequestedCheckOut = table.Column<TimeOnly>(type: "time", nullable: true),
                    Reason = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Status = table.Column<byte>(type: "tinyint", nullable: false),
                    CurrentApprovalLevel = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AttendanceAdjustmentRequests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AttendanceAdjustmentRequests_Employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "OvertimeRequests",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EmployeeId = table.Column<int>(type: "int", nullable: false),
                    WorkDate = table.Column<DateOnly>(type: "date", nullable: false),
                    StartTime = table.Column<TimeOnly>(type: "time", nullable: false),
                    EndTime = table.Column<TimeOnly>(type: "time", nullable: false),
                    TotalHours = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Reason = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Status = table.Column<byte>(type: "tinyint", nullable: false),
                    CurrentApprovalLevel = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OvertimeRequests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OvertimeRequests_Employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AttendanceAdjustmentRequests_EmployeeId",
                table: "AttendanceAdjustmentRequests",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_OvertimeRequests_EmployeeId",
                table: "OvertimeRequests",
                column: "EmployeeId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AttendanceAdjustmentRequests");

            migrationBuilder.DropTable(
                name: "OvertimeRequests");

            migrationBuilder.DropColumn(
                name: "EndTime",
                table: "LeaveRequests");

            migrationBuilder.DropColumn(
                name: "IsFullDay",
                table: "LeaveRequests");

            migrationBuilder.DropColumn(
                name: "StartTime",
                table: "LeaveRequests");

            migrationBuilder.DropColumn(
                name: "TotalHours",
                table: "LeaveRequests");
        }
    }
}
