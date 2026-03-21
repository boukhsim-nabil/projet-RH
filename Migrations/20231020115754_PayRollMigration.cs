using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace itgsgroup.Migrations
{
    /// <inheritdoc />
    public partial class PayRollMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "payRolls",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    monthYear = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    employeeId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    employeeName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    gross_salary = table.Column<int>(type: "int", nullable: true),
                    basic = table.Column<int>(type: "int", nullable: true),
                    hra = table.Column<int>(type: "int", nullable: true),
                    medical_all = table.Column<int>(type: "int", nullable: true),
                    con_alll = table.Column<int>(type: "int", nullable: true),
                    food_all = table.Column<int>(type: "int", nullable: true),
                    other_all = table.Column<int>(type: "int", nullable: true),
                    days = table.Column<int>(type: "int", nullable: true),
                    month_salary = table.Column<int>(type: "int", nullable: true),
                    deduction_count = table.Column<int>(type: "int", nullable: true),
                    att_deduction = table.Column<int>(type: "int", nullable: true),
                    pf = table.Column<int>(type: "int", nullable: true),
                    EOBI = table.Column<int>(type: "int", nullable: true),
                    joining = table.Column<DateTime>(type: "datetime2", nullable: true),
                    incometax = table.Column<int>(type: "int", nullable: true),
                    loan_deduction = table.Column<int>(type: "int", nullable: true),
                    sessi_deduction = table.Column<int>(type: "int", nullable: true),
                    other_deduction = table.Column<int>(type: "int", nullable: true),
                    arear = table.Column<int>(type: "int", nullable: true),
                    bonus = table.Column<int>(type: "int", nullable: true),
                    remarks = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CPR = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    total_deduction = table.Column<int>(type: "int", nullable: true),
                    total_addition = table.Column<int>(type: "int", nullable: true),
                    net_salary = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_payRolls", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "payRolls");
        }
    }
}
