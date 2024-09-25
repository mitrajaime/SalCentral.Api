using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SalCentral.Api.Migrations
{
    public partial class payrolldetails_field : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "PagIbigContribution",
                table: "PayrollDetails",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "PhilHealthContribution",
                table: "PayrollDetails",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "SSSContribution",
                table: "PayrollDetails",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PagIbigContribution",
                table: "PayrollDetails");

            migrationBuilder.DropColumn(
                name: "PhilHealthContribution",
                table: "PayrollDetails");

            migrationBuilder.DropColumn(
                name: "SSSContribution",
                table: "PayrollDetails");
        }
    }
}
