using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RussianSitesStatus.Database.Migrations
{
    public partial class AddedCheckIteration : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "Iteration",
                table: "Checks",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Iteration",
                table: "Checks");
        }
    }
}
