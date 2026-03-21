using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace itgsgroup.Migrations
{
    /// <inheritdoc />
    public partial class updatetempMonthAttmigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "absent",
                table: "tempMonthAtts",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "diciplinaryaction",
                table: "tempMonthAtts",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "late",
                table: "tempMonthAtts",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "absent",
                table: "tempMonthAtts");

            migrationBuilder.DropColumn(
                name: "diciplinaryaction",
                table: "tempMonthAtts");

            migrationBuilder.DropColumn(
                name: "late",
                table: "tempMonthAtts");
        }
    }
}
