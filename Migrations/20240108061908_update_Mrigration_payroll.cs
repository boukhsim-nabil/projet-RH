using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace itgsgroup.Migrations
{
    /// <inheritdoc />
    public partial class update_Mrigration_payroll : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_diciplinaryActions_companies_companyId",
                table: "diciplinaryActions");

            migrationBuilder.AddColumn<int>(
                name: "taxable_arear",
                table: "payRolls",
                type: "int",
                nullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "companyId",
                table: "diciplinaryActions",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddForeignKey(
                name: "FK_diciplinaryActions_companies_companyId",
                table: "diciplinaryActions",
                column: "companyId",
                principalTable: "companies",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_diciplinaryActions_companies_companyId",
                table: "diciplinaryActions");

            migrationBuilder.DropColumn(
                name: "taxable_arear",
                table: "payRolls");

            migrationBuilder.AlterColumn<int>(
                name: "companyId",
                table: "diciplinaryActions",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_diciplinaryActions_companies_companyId",
                table: "diciplinaryActions",
                column: "companyId",
                principalTable: "companies",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
