using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RussianSitesStatus.Database.Migrations
{
    public partial class RemovedCalcStatsProcedure : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"DELETE FROM checks_statistics;");
            migrationBuilder.Sql(@"DROP FUNCTION IF EXISTS fn_calculate_statistic_per_day (site_id_p bigint, day DATE);");            
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                CREATE OR REPLACE FUNCTION fn_calculate_statistic_per_day (
                    site_id_p bigint,
                    day DATE
                ) RETURNS TABLE (
                    hour int,
                    avg_time int,
                    up int,
                    unknown int,
                    down int
                )
                LANGUAGE plpgsql
                AS
                $$
                BEGIN
                    RETURN QUERY
                      SELECT CAST(date_part('hour', checked_at) AS int),
                             CAST(AVG(spent_time) as int ),
                             SUM(CASE status WHEN 1 THEN 1 ELSE 0 END)::int, --AS Available
                             SUM(CASE status WHEN 2 THEN 1 ELSE 0 END)::int, --AS Unknown
                             SUM(CASE status WHEN 3 THEN 1 ELSE 0 END)::int  --AS Down
                       FROM checks
                       WHERE day < checked_at AND checked_at < (day + interval '1 day') AND site_id = site_id_p
                    GROUP BY date_part('hour', checked_at)
                    ORDER BY date_part('hour', checked_at);
                END
                $$;
            ");
        }
    }
}
