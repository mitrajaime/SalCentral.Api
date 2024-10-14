using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SalCentral.Api.Migrations
{
    public partial class RenamingSalaryColumnUsser : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Salary",
                table: "User",
                newName: "SalaryRate");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Salary",
                table: "User",
                newName: "SalaryRate");
        }

    }
}
