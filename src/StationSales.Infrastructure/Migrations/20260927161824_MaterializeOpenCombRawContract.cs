using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StationSales.Infrastructure.Migrations
{
    public partial class MaterializeOpenCombRawContract : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "art_descripcion",
                schema: "raw",
                table: "opencomb_station_sale",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "at",
                schema: "raw",
                table: "opencomb_station_sale",
                type: "nvarchar(1)",
                maxLength: 1,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "caja",
                schema: "raw",
                table: "opencomb_station_sale",
                type: "nvarchar(2)",
                maxLength: 2,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "cajero",
                schema: "raw",
                table: "opencomb_station_sale",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "cantidad",
                schema: "raw",
                table: "opencomb_station_sale",
                type: "decimal(20,4)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "codigo",
                schema: "raw",
                table: "opencomb_station_sale",
                type: "nvarchar(13)",
                maxLength: 13,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "dia",
                schema: "raw",
                table: "opencomb_station_sale",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ebidata",
                schema: "raw",
                table: "opencomb_station_sale",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "es",
                schema: "raw",
                table: "opencomb_station_sale",
                type: "nvarchar(3)",
                maxLength: 3,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "fecha",
                schema: "raw",
                table: "opencomb_station_sale",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "fecha_replicacion",
                schema: "raw",
                table: "opencomb_station_sale",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "flg_replicacion",
                schema: "raw",
                table: "opencomb_station_sale",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "fpago",
                schema: "raw",
                table: "opencomb_station_sale",
                type: "nvarchar(1)",
                maxLength: 1,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "grupo",
                schema: "raw",
                table: "opencomb_station_sale",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "igv",
                schema: "raw",
                table: "opencomb_station_sale",
                type: "decimal(20,4)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "importe",
                schema: "raw",
                table: "opencomb_station_sale",
                type: "decimal(20,4)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "km",
                schema: "raw",
                table: "opencomb_station_sale",
                type: "decimal(20,4)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "nombre",
                schema: "raw",
                table: "opencomb_station_sale",
                type: "nvarchar(15)",
                maxLength: 15,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "placa",
                schema: "raw",
                table: "opencomb_station_sale",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "precio",
                schema: "raw",
                table: "opencomb_station_sale",
                type: "decimal(20,4)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "pump",
                schema: "raw",
                table: "opencomb_station_sale",
                type: "nvarchar(2)",
                maxLength: 2,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "razon_social",
                schema: "raw",
                table: "opencomb_station_sale",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "rendi_acu",
                schema: "raw",
                table: "opencomb_station_sale",
                type: "decimal(20,4)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "rendi_gln",
                schema: "raw",
                table: "opencomb_station_sale",
                type: "decimal(20,4)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ruc",
                schema: "raw",
                table: "opencomb_station_sale",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "soles_km",
                schema: "raw",
                table: "opencomb_station_sale",
                type: "decimal(20,4)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "source_table",
                schema: "raw",
                table: "opencomb_station_sale",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "tarjeta",
                schema: "raw",
                table: "opencomb_station_sale",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "td",
                schema: "raw",
                table: "opencomb_station_sale",
                type: "nvarchar(1)",
                maxLength: 1,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "text1",
                schema: "raw",
                table: "opencomb_station_sale",
                type: "nvarchar(16)",
                maxLength: 16,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "tipo",
                schema: "raw",
                table: "opencomb_station_sale",
                type: "nvarchar(1)",
                maxLength: 1,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "tm",
                schema: "raw",
                table: "opencomb_station_sale",
                type: "nvarchar(1)",
                maxLength: 1,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "trans",
                schema: "raw",
                table: "opencomb_station_sale",
                type: "decimal(14,0)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "turno",
                schema: "raw",
                table: "opencomb_station_sale",
                type: "nvarchar(1)",
                maxLength: 1,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "usr",
                schema: "raw",
                table: "opencomb_station_sale",
                type: "nvarchar(15)",
                maxLength: 15,
                nullable: true);

            // Preserve already extracted rows before removing the former transport JSON column.
            migrationBuilder.Sql(@"
                UPDATE [raw].[opencomb_station_sale]
                SET [source_table] = JSON_VALUE([Payload], '$.source_table'),
                    [art_descripcion] = JSON_VALUE([Payload], '$.art_descripcion'),
                    [razon_social] = JSON_VALUE([Payload], '$.razon_social'),
                    [ruc] = JSON_VALUE([Payload], '$.ruc'),
                    [tm] = JSON_VALUE([Payload], '$.tm'), [caja] = JSON_VALUE([Payload], '$.caja'),
                    [td] = JSON_VALUE([Payload], '$.td'), [dia] = TRY_CONVERT(datetime2, JSON_VALUE([Payload], '$.dia'), 126),
                    [turno] = JSON_VALUE([Payload], '$.turno'), [grupo] = JSON_VALUE([Payload], '$.grupo'),
                    [codigo] = JSON_VALUE([Payload], '$.codigo'), [cantidad] = TRY_CONVERT(decimal(20,4), JSON_VALUE([Payload], '$.cantidad')),
                    [precio] = TRY_CONVERT(decimal(20,4), JSON_VALUE([Payload], '$.precio')), [igv] = TRY_CONVERT(decimal(20,4), JSON_VALUE([Payload], '$.igv')),
                    [importe] = TRY_CONVERT(decimal(20,4), JSON_VALUE([Payload], '$.importe')), [cajero] = JSON_VALUE([Payload], '$.cajero'),
                    [fecha] = TRY_CONVERT(datetime2, JSON_VALUE([Payload], '$.fecha'), 126), [tipo] = JSON_VALUE([Payload], '$.tipo'),
                    [pump] = JSON_VALUE([Payload], '$.pump'), [fpago] = JSON_VALUE([Payload], '$.fpago'), [at] = JSON_VALUE([Payload], '$.at'),
                    [text1] = JSON_VALUE([Payload], '$.text1'), [tarjeta] = JSON_VALUE([Payload], '$.tarjeta'), [km] = TRY_CONVERT(decimal(20,4), JSON_VALUE([Payload], '$.km')),
                    [rendi_gln] = TRY_CONVERT(decimal(20,4), JSON_VALUE([Payload], '$.rendi_gln')), [rendi_acu] = TRY_CONVERT(decimal(20,4), JSON_VALUE([Payload], '$.rendi_acu')),
                    [soles_km] = TRY_CONVERT(decimal(20,4), JSON_VALUE([Payload], '$.soles_km')), [placa] = JSON_VALUE([Payload], '$.placa'),
                    [nombre] = JSON_VALUE([Payload], '$.nombre'), [usr] = JSON_VALUE([Payload], '$.usr'), [es] = JSON_VALUE([Payload], '$.es'),
                    [flg_replicacion] = TRY_CONVERT(int, JSON_VALUE([Payload], '$.flg_replicacion')),
                    [fecha_replicacion] = TRY_CONVERT(datetime2, JSON_VALUE([Payload], '$.fecha_replicacion'), 126),
                    [trans] = TRY_CONVERT(decimal(14,0), JSON_VALUE([Payload], '$.trans')), [ebidata] = JSON_VALUE([Payload], '$.ebidata')
                WHERE [Payload] IS NOT NULL;");

            migrationBuilder.DropColumn(
                name: "Payload",
                schema: "raw",
                table: "opencomb_station_sale");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "art_descripcion",
                schema: "raw",
                table: "opencomb_station_sale");

            migrationBuilder.DropColumn(
                name: "at",
                schema: "raw",
                table: "opencomb_station_sale");

            migrationBuilder.DropColumn(
                name: "caja",
                schema: "raw",
                table: "opencomb_station_sale");

            migrationBuilder.DropColumn(
                name: "cajero",
                schema: "raw",
                table: "opencomb_station_sale");

            migrationBuilder.DropColumn(
                name: "cantidad",
                schema: "raw",
                table: "opencomb_station_sale");

            migrationBuilder.DropColumn(
                name: "codigo",
                schema: "raw",
                table: "opencomb_station_sale");

            migrationBuilder.DropColumn(
                name: "dia",
                schema: "raw",
                table: "opencomb_station_sale");

            migrationBuilder.DropColumn(
                name: "ebidata",
                schema: "raw",
                table: "opencomb_station_sale");

            migrationBuilder.DropColumn(
                name: "es",
                schema: "raw",
                table: "opencomb_station_sale");

            migrationBuilder.DropColumn(
                name: "fecha",
                schema: "raw",
                table: "opencomb_station_sale");

            migrationBuilder.DropColumn(
                name: "fecha_replicacion",
                schema: "raw",
                table: "opencomb_station_sale");

            migrationBuilder.DropColumn(
                name: "flg_replicacion",
                schema: "raw",
                table: "opencomb_station_sale");

            migrationBuilder.DropColumn(
                name: "fpago",
                schema: "raw",
                table: "opencomb_station_sale");

            migrationBuilder.DropColumn(
                name: "grupo",
                schema: "raw",
                table: "opencomb_station_sale");

            migrationBuilder.DropColumn(
                name: "igv",
                schema: "raw",
                table: "opencomb_station_sale");

            migrationBuilder.DropColumn(
                name: "importe",
                schema: "raw",
                table: "opencomb_station_sale");

            migrationBuilder.DropColumn(
                name: "km",
                schema: "raw",
                table: "opencomb_station_sale");

            migrationBuilder.DropColumn(
                name: "nombre",
                schema: "raw",
                table: "opencomb_station_sale");

            migrationBuilder.DropColumn(
                name: "placa",
                schema: "raw",
                table: "opencomb_station_sale");

            migrationBuilder.DropColumn(
                name: "precio",
                schema: "raw",
                table: "opencomb_station_sale");

            migrationBuilder.DropColumn(
                name: "pump",
                schema: "raw",
                table: "opencomb_station_sale");

            migrationBuilder.DropColumn(
                name: "razon_social",
                schema: "raw",
                table: "opencomb_station_sale");

            migrationBuilder.DropColumn(
                name: "rendi_acu",
                schema: "raw",
                table: "opencomb_station_sale");

            migrationBuilder.DropColumn(
                name: "rendi_gln",
                schema: "raw",
                table: "opencomb_station_sale");

            migrationBuilder.DropColumn(
                name: "ruc",
                schema: "raw",
                table: "opencomb_station_sale");

            migrationBuilder.DropColumn(
                name: "soles_km",
                schema: "raw",
                table: "opencomb_station_sale");

            migrationBuilder.DropColumn(
                name: "source_table",
                schema: "raw",
                table: "opencomb_station_sale");

            migrationBuilder.DropColumn(
                name: "tarjeta",
                schema: "raw",
                table: "opencomb_station_sale");

            migrationBuilder.DropColumn(
                name: "td",
                schema: "raw",
                table: "opencomb_station_sale");

            migrationBuilder.DropColumn(
                name: "text1",
                schema: "raw",
                table: "opencomb_station_sale");

            migrationBuilder.DropColumn(
                name: "tipo",
                schema: "raw",
                table: "opencomb_station_sale");

            migrationBuilder.DropColumn(
                name: "tm",
                schema: "raw",
                table: "opencomb_station_sale");

            migrationBuilder.DropColumn(
                name: "trans",
                schema: "raw",
                table: "opencomb_station_sale");

            migrationBuilder.DropColumn(
                name: "turno",
                schema: "raw",
                table: "opencomb_station_sale");

            migrationBuilder.DropColumn(
                name: "usr",
                schema: "raw",
                table: "opencomb_station_sale");

            migrationBuilder.AddColumn<string>(
                name: "Payload",
                schema: "raw",
                table: "opencomb_station_sale",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
