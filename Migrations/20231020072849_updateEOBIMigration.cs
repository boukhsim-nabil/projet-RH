using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace itgsgroup.Migrations
{
    /// <inheritdoc />
    public partial class updateEOBIMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "FYId",
                table: "EOBIs",
                newName: "fyId");

            migrationBuilder.CreateIndex(
                name: "IX_EOBIs_fyId",
                table: "EOBIs",
                column: "fyId");

            migrationBuilder.AddForeignKey(
                name: "FK_EOBIs_fascalYears_fyId",
                table: "EOBIs",
                column: "fyId",
                principalTable: "fascalYears",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EOBIs_fascalYears_fyId",
                table: "EOBIs");

            migrationBuilder.DropIndex(
                name: "IX_EOBIs_fyId",
                table: "EOBIs");

            migrationBuilder.RenameColumn(
                name: "fyId",
                table: "EOBIs",
                newName: "FYId");
        }
    }
}
