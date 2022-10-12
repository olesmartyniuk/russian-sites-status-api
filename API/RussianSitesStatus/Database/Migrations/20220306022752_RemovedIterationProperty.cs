using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RussianSitesStatus.Database.Migrations
{
    public partial class RemovedIterationProperty : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Iteration",
                table: "Checks");

            migrationBuilder.CreateIndex(
                name: "IX_Checks_CheckedAt",
                table: "Checks",
                column: "CheckedAt");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Checks_CheckedAt",
                table: "Checks");

            migrationBuilder.AddColumn<Guid>(
                name: "Iteration",
                table: "Checks",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));
        }
    }
}
