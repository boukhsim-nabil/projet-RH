using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace itgsgroup.Migrations
{
    /// <inheritdoc />
    public partial class updateTempatt2Mihration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "earlygoing",
                table: "tempMonthAtts",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "halfday",
                table: "tempMonthAtts",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "present",
                table: "tempMonthAtts",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "earlygoing",
                table: "tempMonthAtts");

            migrationBuilder.DropColumn(
                name: "halfday",
                table: "tempMonthAtts");

            migrationBuilder.DropColumn(
                name: "present",
                table: "tempMonthAtts");
        }
    }
}
