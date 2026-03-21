using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace itgsgroup.Migrations
{
    /// <inheritdoc />
    public partial class update2migration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_rawattendances_companies_companyId",
                table: "rawattendances");

            migrationBuilder.DropIndex(
                name: "IX_rawattendances_companyId",
                table: "rawattendances");

            migrationBuilder.DropColumn(
                name: "companyId",
                table: "rawattendances");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "companyId",
                table: "rawattendances",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_rawattendances_companyId",
                table: "rawattendances",
                column: "companyId");

            migrationBuilder.AddForeignKey(
                name: "FK_rawattendances_companies_companyId",
                table: "rawattendances",
                column: "companyId",
                principalTable: "companies",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
