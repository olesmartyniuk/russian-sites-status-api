using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace RussianSitesStatus.Database.Migrations
{
    public partial class AddedRegionModel : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Region",
                table: "Checks");

            migrationBuilder.AddColumn<long>(
                name: "RegionId",
                table: "Checks",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.CreateTable(
                name: "Regions",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: false),
                    ProxyUrl = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Regions", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Checks_RegionId",
                table: "Checks",
                column: "RegionId");

            migrationBuilder.AddForeignKey(
                name: "FK_Checks_Regions_RegionId",
                table: "Checks",
                column: "RegionId",
                principalTable: "Regions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Checks_Regions_RegionId",
                table: "Checks");

            migrationBuilder.DropTable(
                name: "Regions");

            migrationBuilder.DropIndex(
                name: "IX_Checks_RegionId",
                table: "Checks");

            migrationBuilder.DropColumn(
                name: "RegionId",
                table: "Checks");

            migrationBuilder.AddColumn<int>(
                name: "Region",
                table: "Checks",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }
    }
}
