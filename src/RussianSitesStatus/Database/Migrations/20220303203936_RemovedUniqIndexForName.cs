using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RussianSitesStatus.Database.Migrations
{
    public partial class RemovedUniqIndexForName : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Sites_Name",
                table: "Sites");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Sites_Name",
                table: "Sites",
                column: "Name",
                unique: true);
        }
    }
}
