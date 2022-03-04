using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RussianSitesStatus.Database.Migrations
{
    public partial class AddedProxyActiveProperty : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "ProxyIsActive",
                table: "Regions",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ProxyIsActive",
                table: "Regions");
        }
    }
}
