using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SalCentral.Api.Migrations
{
    public partial class withSalaryRate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "SalaryRate",
                table: "User",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "CurrentSalaryRate",
                table: "PayrollDetails",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AlterColumn<decimal>(
                name: "Amount",
                table: "Deduction",
                type: "decimal(18,2)",
                nullable: false,
                oldClrType: typeof(double),
                oldType: "float");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SalaryRate",
                table: "User");

            migrationBuilder.DropColumn(
                name: "CurrentSalaryRate",
                table: "PayrollDetails");

            migrationBuilder.AlterColumn<double>(
                name: "Amount",
                table: "Deduction",
                type: "float",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");
        }
    }
}
