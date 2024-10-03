using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SalCentral.Api.Migrations
{
    public partial class updatedpayroll : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsPaid",
                table: "Payroll");

            migrationBuilder.AddColumn<bool>(
                name: "IsPaid",
                table: "PayrollDetails",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsPaid",
                table: "PayrollDetails");

            migrationBuilder.AddColumn<bool>(
                name: "IsPaid",
                table: "Payroll",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }
    }
}
