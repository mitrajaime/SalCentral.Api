using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SalCentral.Api.Migrations
{
    public partial class addedIsMandatoryToDeduction : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BranchId",
                table: "Deduction");

            migrationBuilder.AddColumn<decimal>(
                name: "HolidayPay",
                table: "PayrollDetails",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "OvertimePay",
                table: "PayrollDetails",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);  

            migrationBuilder.AddColumn<bool>(
                name: "IsMandatory",
                table: "Deduction",
                type: "bit",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HolidayPay",
                table: "PayrollDetails");

            migrationBuilder.DropColumn(
                name: "OvertimePay",
                table: "PayrollDetails");

            migrationBuilder.DropColumn(
                name: "IsMandatory",
                table: "Deduction");

            migrationBuilder.AddColumn<Guid>(
                name: "BranchId",
                table: "Deduction",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));
        }
    }
}
