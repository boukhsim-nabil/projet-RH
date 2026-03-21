using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace itgsgroup.Migrations
{
    /// <inheritdoc />
    public partial class loanapplymigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "loanApplies",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    loanamount = table.Column<int>(type: "int", nullable: false),
                    repaymentamount = table.Column<int>(type: "int", nullable: false),
                    startdate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    reason = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    empId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    companyId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_loanApplies", x => x.Id);
                    table.ForeignKey(
                        name: "FK_loanApplies_AspNetUsers_empId",
                        column: x => x.empId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_loanApplies_companies_companyId",
                        column: x => x.companyId,
                        principalTable: "companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_loanApplies_companyId",
                table: "loanApplies",
                column: "companyId");

            migrationBuilder.CreateIndex(
                name: "IX_loanApplies_empId",
                table: "loanApplies",
                column: "empId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "loanApplies");
        }
    }
}
