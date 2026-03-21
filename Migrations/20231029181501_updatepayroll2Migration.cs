using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace itgsgroup.Migrations
{
    /// <inheritdoc />
    public partial class updatepayroll2Migration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "companyId",
                table: "payRolls",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_payRolls_companyId",
                table: "payRolls",
                column: "companyId");

            migrationBuilder.AddForeignKey(
                name: "FK_payRolls_companies_companyId",
                table: "payRolls",
                column: "companyId",
                principalTable: "companies",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_payRolls_companies_companyId",
                table: "payRolls");

            migrationBuilder.DropIndex(
                name: "IX_payRolls_companyId",
                table: "payRolls");

            migrationBuilder.DropColumn(
                name: "companyId",
                table: "payRolls");
        }
    }
}
