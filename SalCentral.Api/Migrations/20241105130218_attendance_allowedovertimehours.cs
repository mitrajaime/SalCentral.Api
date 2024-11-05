using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SalCentral.Api.Migrations
{
    public partial class attendance_allowedovertimehours : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AllowedOvertimeHours",
                table: "Attendance",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AllowedOvertimeHours",
                table: "Attendance");
        }
    }
}
