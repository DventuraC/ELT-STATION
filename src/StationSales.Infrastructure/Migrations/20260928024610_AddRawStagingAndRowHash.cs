using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StationSales.Infrastructure.Migrations
{
    public partial class AddRawStagingAndRowHash : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<byte[]>(
                name: "RowHash",
                schema: "raw",
                table: "opencomb_station_sale",
                type: "binary(32)",
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowHash",
                schema: "raw",
                table: "gasolution_station_sale",
                type: "binary(32)",
                nullable: true);

            migrationBuilder.Sql(@"
SELECT TOP (0) * INTO [raw].[opencomb_station_sale_staging]
FROM [raw].[opencomb_station_sale];

SELECT TOP (0) * INTO [raw].[gasolution_station_sale_staging]
FROM [raw].[gasolution_station_sale];
");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RowHash",
                schema: "raw",
                table: "opencomb_station_sale");

            migrationBuilder.DropColumn(
                name: "RowHash",
                schema: "raw",
                table: "gasolution_station_sale");

            migrationBuilder.Sql(@"
DROP TABLE IF EXISTS [raw].[opencomb_station_sale_staging];
DROP TABLE IF EXISTS [raw].[gasolution_station_sale_staging];
");
        }
    }
}
