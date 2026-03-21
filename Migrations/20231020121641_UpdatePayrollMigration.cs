using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace itgsgroup.Migrations
{
    /// <inheritdoc />
    public partial class UpdatePayrollMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "month_salary",
                table: "payRolls",
                newName: "utility_all");

            migrationBuilder.RenameColumn(
                name: "con_alll",
                table: "payRolls",
                newName: "day_salary");

            migrationBuilder.AddColumn<int>(
                name: "con_all",
                table: "payRolls",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "con_all",
                table: "payRolls");

            migrationBuilder.RenameColumn(
                name: "utility_all",
                table: "payRolls",
                newName: "month_salary");

            migrationBuilder.RenameColumn(
                name: "day_salary",
                table: "payRolls",
                newName: "con_alll");
        }
    }
}
