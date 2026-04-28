using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HRM.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FixPendingModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AttendanceAdjustmentRequests");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AttendanceAdjustmentRequests",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EmployeeId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CurrentApprovalLevel = table.Column<int>(type: "int", nullable: false),
                    Reason = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RequestedCheckIn = table.Column<TimeOnly>(type: "time", nullable: true),
                    RequestedCheckOut = table.Column<TimeOnly>(type: "time", nullable: true),
                    Status = table.Column<byte>(type: "tinyint", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    WorkDate = table.Column<DateOnly>(type: "date", nullable: false)
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

            migrationBuilder.CreateIndex(
                name: "IX_AttendanceAdjustmentRequests_EmployeeId",
                table: "AttendanceAdjustmentRequests",
                column: "EmployeeId");
        }
    }
}
