using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StationSales.Infrastructure.Migrations
{
    public partial class AddSourceSchedulingInterval : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "IntervalMinutes",
                schema: "etl",
                table: "pipeline_data_source",
                type: "int",
                nullable: false,
                defaultValue: 60);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IntervalMinutes",
                schema: "etl",
                table: "pipeline_data_source");
        }
    }
}
