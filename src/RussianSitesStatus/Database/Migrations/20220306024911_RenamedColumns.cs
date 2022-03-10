using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RussianSitesStatus.Database.Migrations
{
    public partial class RenamedColumns : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Code",
                table: "regions",
                newName: "code");

            migrationBuilder.RenameColumn(
                name: "ProxyIsActive",
                table: "regions",
                newName: "proxy_is_active");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "code",
                table: "regions",
                newName: "Code");

            migrationBuilder.RenameColumn(
                name: "proxy_is_active",
                table: "regions",
                newName: "ProxyIsActive");
        }
    }
}
