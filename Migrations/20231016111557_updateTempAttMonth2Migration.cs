using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace itgsgroup.Migrations
{
    /// <inheritdoc />
    public partial class updateTempAttMonth2Migration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "R_Checkin",
                table: "tempMonthAtts",
                type: "datetime2",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_tempMonthAtts_companyId",
                table: "tempMonthAtts",
                column: "companyId");

            migrationBuilder.AddForeignKey(
                name: "FK_tempMonthAtts_companies_companyId",
                table: "tempMonthAtts",
                column: "companyId",
                principalTable: "companies",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_tempMonthAtts_companies_companyId",
                table: "tempMonthAtts");

            migrationBuilder.DropIndex(
                name: "IX_tempMonthAtts_companyId",
                table: "tempMonthAtts");

            migrationBuilder.DropColumn(
                name: "R_Checkin",
                table: "tempMonthAtts");
        }
    }
}
