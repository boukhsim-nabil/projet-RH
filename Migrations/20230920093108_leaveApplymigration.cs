using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace itgsgroup.Migrations
{
    /// <inheritdoc />
    public partial class leaveApplymigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_empDocs_AspNetUsers_empId",
                table: "empDocs");

            migrationBuilder.DropForeignKey(
                name: "FK_empDocs_companies_companyId",
                table: "empDocs");

            migrationBuilder.DropForeignKey(
                name: "FK_empFamily_AspNetUsers_empId",
                table: "empFamily");

            migrationBuilder.DropForeignKey(
                name: "FK_empFamily_companies_companyId",
                table: "empFamily");

            migrationBuilder.DropForeignKey(
                name: "FK_empFamilyDocs_AspNetUsers_empId",
                table: "empFamilyDocs");

            migrationBuilder.DropForeignKey(
                name: "FK_empFamilyDocs_companies_companyId",
                table: "empFamilyDocs");

            migrationBuilder.DropForeignKey(
                name: "FK_leaveTypes_companies_companyId",
                table: "leaveTypes");

            migrationBuilder.DropForeignKey(
                name: "FK_leaveTypes_fascalYears_fyId",
                table: "leaveTypes");

            migrationBuilder.DropForeignKey(
                name: "FK_slabs_companies_companyId",
                table: "slabs");

            migrationBuilder.DropForeignKey(
                name: "FK_slabs_fascalYears_fyId",
                table: "slabs");

            migrationBuilder.AlterColumn<DateTime>(
                name: "cnic_expiry",
                table: "empFamily",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AlterColumn<DateTime>(
                name: "DOB",
                table: "empFamily",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.CreateTable(
                name: "leaveApplies",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    from = table.Column<DateTime>(type: "datetime2", nullable: false),
                    to = table.Column<DateTime>(type: "datetime2", nullable: false),
                    leaveId = table.Column<int>(type: "int", nullable: false),
                    reason = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    empId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    companyId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_leaveApplies", x => x.Id);
                    table.ForeignKey(
                        name: "FK_leaveApplies_AspNetUsers_empId",
                        column: x => x.empId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_leaveApplies_companies_companyId",
                        column: x => x.companyId,
                        principalTable: "companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_leaveApplies_leaveTypes_leaveId",
                        column: x => x.leaveId,
                        principalTable: "leaveTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_leaveApplies_companyId",
                table: "leaveApplies",
                column: "companyId");

            migrationBuilder.CreateIndex(
                name: "IX_leaveApplies_empId",
                table: "leaveApplies",
                column: "empId");

            migrationBuilder.CreateIndex(
                name: "IX_leaveApplies_leaveId",
                table: "leaveApplies",
                column: "leaveId");

            migrationBuilder.AddForeignKey(
                name: "FK_empDocs_AspNetUsers_empId",
                table: "empDocs",
                column: "empId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_empDocs_companies_companyId",
                table: "empDocs",
                column: "companyId",
                principalTable: "companies",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_empFamily_AspNetUsers_empId",
                table: "empFamily",
                column: "empId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_empFamily_companies_companyId",
                table: "empFamily",
                column: "companyId",
                principalTable: "companies",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_empFamilyDocs_AspNetUsers_empId",
                table: "empFamilyDocs",
                column: "empId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_empFamilyDocs_companies_companyId",
                table: "empFamilyDocs",
                column: "companyId",
                principalTable: "companies",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_leaveTypes_companies_companyId",
                table: "leaveTypes",
                column: "companyId",
                principalTable: "companies",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_leaveTypes_fascalYears_fyId",
                table: "leaveTypes",
                column: "fyId",
                principalTable: "fascalYears",
                principalColumn: "Id"
                );

            migrationBuilder.AddForeignKey(
                name: "FK_slabs_companies_companyId",
                table: "slabs",
                column: "companyId",
                principalTable: "companies",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_slabs_fascalYears_fyId",
                table: "slabs",
                column: "fyId",
                principalTable: "fascalYears",
                principalColumn: "Id"
                );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_empDocs_AspNetUsers_empId",
                table: "empDocs");

            migrationBuilder.DropForeignKey(
                name: "FK_empDocs_companies_companyId",
                table: "empDocs");

            migrationBuilder.DropForeignKey(
                name: "FK_empFamily_AspNetUsers_empId",
                table: "empFamily");

            migrationBuilder.DropForeignKey(
                name: "FK_empFamily_companies_companyId",
                table: "empFamily");

            migrationBuilder.DropForeignKey(
                name: "FK_empFamilyDocs_AspNetUsers_empId",
                table: "empFamilyDocs");

            migrationBuilder.DropForeignKey(
                name: "FK_empFamilyDocs_companies_companyId",
                table: "empFamilyDocs");

            migrationBuilder.DropForeignKey(
                name: "FK_leaveTypes_companies_companyId",
                table: "leaveTypes");

            migrationBuilder.DropForeignKey(
                name: "FK_leaveTypes_fascalYears_fyId",
                table: "leaveTypes");

            migrationBuilder.DropForeignKey(
                name: "FK_slabs_companies_companyId",
                table: "slabs");

            migrationBuilder.DropForeignKey(
                name: "FK_slabs_fascalYears_fyId",
                table: "slabs");

            migrationBuilder.DropTable(
                name: "leaveApplies");

            migrationBuilder.AlterColumn<DateTime>(
                name: "cnic_expiry",
                table: "empFamily",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "DOB",
                table: "empFamily",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_empDocs_AspNetUsers_empId",
                table: "empDocs",
                column: "empId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_empDocs_companies_companyId",
                table: "empDocs",
                column: "companyId",
                principalTable: "companies",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_empFamily_AspNetUsers_empId",
                table: "empFamily",
                column: "empId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_empFamily_companies_companyId",
                table: "empFamily",
                column: "companyId",
                principalTable: "companies",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_empFamilyDocs_AspNetUsers_empId",
                table: "empFamilyDocs",
                column: "empId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_empFamilyDocs_companies_companyId",
                table: "empFamilyDocs",
                column: "companyId",
                principalTable: "companies",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_leaveTypes_companies_companyId",
                table: "leaveTypes",
                column: "companyId",
                principalTable: "companies",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_leaveTypes_fascalYears_fyId",
                table: "leaveTypes",
                column: "fyId",
                principalTable: "fascalYears",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_slabs_companies_companyId",
                table: "slabs",
                column: "companyId",
                principalTable: "companies",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_slabs_fascalYears_fyId",
                table: "slabs",
                column: "fyId",
                principalTable: "fascalYears",
                principalColumn: "Id");
        }
    }
}
