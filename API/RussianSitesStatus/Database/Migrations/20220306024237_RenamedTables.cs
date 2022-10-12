using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RussianSitesStatus.Database.Migrations
{
    public partial class RenamedTables : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Checks_Regions_RegionId",
                table: "Checks");

            migrationBuilder.DropForeignKey(
                name: "FK_Checks_Sites_SiteId",
                table: "Checks");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Sites",
                table: "Sites");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Regions",
                table: "Regions");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Checks",
                table: "Checks");

            migrationBuilder.RenameTable(
                name: "Sites",
                newName: "sites");

            migrationBuilder.RenameTable(
                name: "Regions",
                newName: "regions");

            migrationBuilder.RenameTable(
                name: "Checks",
                newName: "checks");

            migrationBuilder.RenameColumn(
                name: "Url",
                table: "sites",
                newName: "url");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "sites",
                newName: "name");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "sites",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "sites",
                newName: "created_at");

            migrationBuilder.RenameIndex(
                name: "IX_Sites_Url",
                table: "sites",
                newName: "IX_sites_url");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "regions",
                newName: "name");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "regions",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "ProxyUser",
                table: "regions",
                newName: "proxy_user");

            migrationBuilder.RenameColumn(
                name: "ProxyUrl",
                table: "regions",
                newName: "proxy_url");

            migrationBuilder.RenameColumn(
                name: "ProxyPassword",
                table: "regions",
                newName: "proxy_password");

            migrationBuilder.RenameColumn(
                name: "Status",
                table: "checks",
                newName: "status");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "checks",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "StatusCode",
                table: "checks",
                newName: "status_code");

            migrationBuilder.RenameColumn(
                name: "SpentTime",
                table: "checks",
                newName: "spent_time");

            migrationBuilder.RenameColumn(
                name: "SiteId",
                table: "checks",
                newName: "site_id");

            migrationBuilder.RenameColumn(
                name: "RegionId",
                table: "checks",
                newName: "region_id");

            migrationBuilder.RenameColumn(
                name: "CheckedAt",
                table: "checks",
                newName: "checked_at");

            migrationBuilder.RenameIndex(
                name: "IX_Checks_SiteId",
                table: "checks",
                newName: "IX_checks_site_id");

            migrationBuilder.RenameIndex(
                name: "IX_Checks_RegionId",
                table: "checks",
                newName: "IX_checks_region_id");

            migrationBuilder.RenameIndex(
                name: "IX_Checks_CheckedAt",
                table: "checks",
                newName: "IX_checks_checked_at");

            migrationBuilder.AddPrimaryKey(
                name: "PK_sites",
                table: "sites",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_regions",
                table: "regions",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_checks",
                table: "checks",
                column: "id");

            migrationBuilder.AddForeignKey(
                name: "FK_checks_regions_region_id",
                table: "checks",
                column: "region_id",
                principalTable: "regions",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_checks_sites_site_id",
                table: "checks",
                column: "site_id",
                principalTable: "sites",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_checks_regions_region_id",
                table: "checks");

            migrationBuilder.DropForeignKey(
                name: "FK_checks_sites_site_id",
                table: "checks");

            migrationBuilder.DropPrimaryKey(
                name: "PK_sites",
                table: "sites");

            migrationBuilder.DropPrimaryKey(
                name: "PK_regions",
                table: "regions");

            migrationBuilder.DropPrimaryKey(
                name: "PK_checks",
                table: "checks");

            migrationBuilder.RenameTable(
                name: "sites",
                newName: "Sites");

            migrationBuilder.RenameTable(
                name: "regions",
                newName: "Regions");

            migrationBuilder.RenameTable(
                name: "checks",
                newName: "Checks");

            migrationBuilder.RenameColumn(
                name: "url",
                table: "Sites",
                newName: "Url");

            migrationBuilder.RenameColumn(
                name: "name",
                table: "Sites",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "Sites",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "created_at",
                table: "Sites",
                newName: "CreatedAt");

            migrationBuilder.RenameIndex(
                name: "IX_sites_url",
                table: "Sites",
                newName: "IX_Sites_Url");

            migrationBuilder.RenameColumn(
                name: "name",
                table: "Regions",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "Regions",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "proxy_user",
                table: "Regions",
                newName: "ProxyUser");

            migrationBuilder.RenameColumn(
                name: "proxy_url",
                table: "Regions",
                newName: "ProxyUrl");

            migrationBuilder.RenameColumn(
                name: "proxy_password",
                table: "Regions",
                newName: "ProxyPassword");

            migrationBuilder.RenameColumn(
                name: "status",
                table: "Checks",
                newName: "Status");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "Checks",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "status_code",
                table: "Checks",
                newName: "StatusCode");

            migrationBuilder.RenameColumn(
                name: "spent_time",
                table: "Checks",
                newName: "SpentTime");

            migrationBuilder.RenameColumn(
                name: "site_id",
                table: "Checks",
                newName: "SiteId");

            migrationBuilder.RenameColumn(
                name: "region_id",
                table: "Checks",
                newName: "RegionId");

            migrationBuilder.RenameColumn(
                name: "checked_at",
                table: "Checks",
                newName: "CheckedAt");

            migrationBuilder.RenameIndex(
                name: "IX_checks_site_id",
                table: "Checks",
                newName: "IX_Checks_SiteId");

            migrationBuilder.RenameIndex(
                name: "IX_checks_region_id",
                table: "Checks",
                newName: "IX_Checks_RegionId");

            migrationBuilder.RenameIndex(
                name: "IX_checks_checked_at",
                table: "Checks",
                newName: "IX_Checks_CheckedAt");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Sites",
                table: "Sites",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Regions",
                table: "Regions",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Checks",
                table: "Checks",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Checks_Regions_RegionId",
                table: "Checks",
                column: "RegionId",
                principalTable: "Regions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Checks_Sites_SiteId",
                table: "Checks",
                column: "SiteId",
                principalTable: "Sites",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
