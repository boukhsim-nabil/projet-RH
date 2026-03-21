using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace itgsgroup.Migrations
{
    /// <inheritdoc />
    public partial class updateASPUserMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "employeeId",
                table: "payRolls",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "attend_type",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_payRolls_employeeId",
                table: "payRolls",
                column: "employeeId");

            migrationBuilder.AddForeignKey(
                name: "FK_payRolls_AspNetUsers_employeeId",
                table: "payRolls",
                column: "employeeId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_payRolls_AspNetUsers_employeeId",
                table: "payRolls");

            migrationBuilder.DropIndex(
                name: "IX_payRolls_employeeId",
                table: "payRolls");

            migrationBuilder.DropColumn(
                name: "attend_type",
                table: "AspNetUsers");

            migrationBuilder.AlterColumn<string>(
                name: "employeeId",
                table: "payRolls",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);
        }
    }
}
