using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RussianSitesStatus.Database.Migrations
{
    public partial class AddRegions : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "regions",
                columns: new[] { "id", "name", "code", "proxy_url", "proxy_is_active"},
                values: new object[] { 1, "Russia / Moscov", "russia", "http://193.124.67.14:3128", true });
            migrationBuilder.InsertData(
                table: "regions",
                columns: new[] { "id", "name", "code", "proxy_url", "proxy_is_active" },
                values: new object[] { 2, "Japan / Tokio", "japan", "http://207.148.97.87:80", true });
            migrationBuilder.InsertData(
                table: "regions",
                columns: new[] { "id", "name", "code", "proxy_url", "proxy_is_active" },
                values: new object[] { 3, "USA / Colorado", "usa", "http://173.248.176.156:80", true });
            migrationBuilder.InsertData(
                table: "regions",
                columns: new[] { "id", "name", "code", "proxy_url", "proxy_is_active" },
                values: new object[] { 4, "Germany / Berlin", "germany", "http://138.201.49.61:5138", true });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "regions",
                keyColumn: "code",
                keyValue: "russia");
            migrationBuilder.DeleteData(
                table: "regions",
                keyColumn: "code",
                keyValue: "japan");
            migrationBuilder.DeleteData(
                table: "regions",
                keyColumn: "code",
                keyValue: "usa");
            migrationBuilder.DeleteData(
                table: "regions",
                keyColumn: "code",
                keyValue: "germany");
        }
    }
}
