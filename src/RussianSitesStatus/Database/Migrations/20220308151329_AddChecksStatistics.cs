using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace RussianSitesStatus.Database.Migrations
{
    public partial class AddChecksStatistics : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "checks_statistics",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    site_id = table.Column<long>(type: "bigint", nullable: false),
                    day = table.Column<DateTime>(type: "date", nullable: false),
                    data = table.Column<string>(type: "jsonb", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_checks_statistics", x => x.id);
                    table.ForeignKey(
                        name: "FK_checks_statistics_sites_site_id",
                        column: x => x.site_id,
                        principalTable: "sites",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_checks_statistics_site_id",
                table: "checks_statistics",
                column: "site_id");

            migrationBuilder.Sql(@"
                CREATE OR REPLACE FUNCTION fn_calculate_statistic_per_day (
                    site_id_p int,
                    day DATE
                ) RETURNS TABLE (
                    region int
                    , hour int
                    , avg_time int
                    , up_number int
                    , unavailable_number int
                    , down_number int
                )
                LANGUAGE plpgsql
                AS
                $$
                BEGIN
                    RETURN QUERY
                      SELECT cast(region_id as int)
                             , CAST(date_part('hour', checked_at) AS int)
                             , CAST(AVG(spent_time) as int )
                             , SUM(CASE status WHEN 1 THEN 1 ELSE 0 END)::int -- AS Available
                             , SUM(CASE status WHEN 2 THEN 1 ELSE 0 END)::int --AS Unknown
                             , SUM(CASE status WHEN 3 THEN 1 ELSE 0 END)::int --AS Down
                        FROM checks
                       WHERE day < checked_at AND checked_at < (day + interval '1 day')
                             AND site_id = site_id_p
                    GROUP BY region_id, date_part('hour', checked_at)
                    ORDER BY region_id, date_part('hour', checked_at);
                END
                $$;
            ");
        }
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "checks_statistics");

            migrationBuilder.Sql(@"DROP FUNCTION fn_calculate_statistic_per_day(integer,date);");
        }
    }
}
