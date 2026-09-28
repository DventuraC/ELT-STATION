using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StationSales.Infrastructure.Migrations
{
    public partial class MaterializeGasolutionRawContract : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Payload",
                schema: "raw",
                table: "gasolution_station_sale");

            migrationBuilder.AddColumn<decimal>(
                name: "BaseImponibleDocumento",
                schema: "raw",
                table: "gasolution_station_sale",
                type: "decimal(20,4)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Cantidad",
                schema: "raw",
                table: "gasolution_station_sale",
                type: "decimal(20,4)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Cara",
                schema: "raw",
                table: "gasolution_station_sale",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Cedula",
                schema: "raw",
                table: "gasolution_station_sale",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CodigoQr",
                schema: "raw",
                table: "gasolution_station_sale",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CodigoSunat",
                schema: "raw",
                table: "gasolution_station_sale",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Descuento",
                schema: "raw",
                table: "gasolution_station_sale",
                type: "decimal(20,4)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Despachador",
                schema: "raw",
                table: "gasolution_station_sale",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "EsDescuentoClienteCredito",
                schema: "raw",
                table: "gasolution_station_sale",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaHora",
                schema: "raw",
                table: "gasolution_station_sale",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaHoraDocumento",
                schema: "raw",
                table: "gasolution_station_sale",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FormaPago",
                schema: "raw",
                table: "gasolution_station_sale",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "HoraFin",
                schema: "raw",
                table: "gasolution_station_sale",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "HoraInicio",
                schema: "raw",
                table: "gasolution_station_sale",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "IdVentaFacturacion",
                schema: "raw",
                table: "gasolution_station_sale",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "ImpuestoDocumento",
                schema: "raw",
                table: "gasolution_station_sale",
                type: "decimal(20,4)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "LecturaFinal",
                schema: "raw",
                table: "gasolution_station_sale",
                type: "decimal(20,4)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "LecturaInicial",
                schema: "raw",
                table: "gasolution_station_sale",
                type: "decimal(20,4)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Manguera",
                schema: "raw",
                table: "gasolution_station_sale",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "NroConfirmacionCdc",
                schema: "raw",
                table: "gasolution_station_sale",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Numero",
                schema: "raw",
                table: "gasolution_station_sale",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "NumeroTurno",
                schema: "raw",
                table: "gasolution_station_sale",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Placa",
                schema: "raw",
                table: "gasolution_station_sale",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Precio",
                schema: "raw",
                table: "gasolution_station_sale",
                type: "decimal(20,4)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "PrecioDocumento",
                schema: "raw",
                table: "gasolution_station_sale",
                type: "decimal(20,4)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Producto",
                schema: "raw",
                table: "gasolution_station_sale",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RazonSocial",
                schema: "raw",
                table: "gasolution_station_sale",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Recaudo",
                schema: "raw",
                table: "gasolution_station_sale",
                type: "decimal(20,4)",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "Recibo",
                schema: "raw",
                table: "gasolution_station_sale",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "RedondeoDonacionDocumento",
                schema: "raw",
                table: "gasolution_station_sale",
                type: "decimal(20,4)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Ruc",
                schema: "raw",
                table: "gasolution_station_sale",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Serie",
                schema: "raw",
                table: "gasolution_station_sale",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SourceDatabase",
                schema: "raw",
                table: "gasolution_station_sale",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "SubtotalDocumento",
                schema: "raw",
                table: "gasolution_station_sale",
                type: "decimal(20,4)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Surtidor",
                schema: "raw",
                table: "gasolution_station_sale",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TipoDocumento",
                schema: "raw",
                table: "gasolution_station_sale",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TipoRedondeo",
                schema: "raw",
                table: "gasolution_station_sale",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "TotalPagarDocumento",
                schema: "raw",
                table: "gasolution_station_sale",
                type: "decimal(20,4)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TramaEnviada",
                schema: "raw",
                table: "gasolution_station_sale",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Valor",
                schema: "raw",
                table: "gasolution_station_sale",
                type: "decimal(20,4)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "ValorCambioPrecio",
                schema: "raw",
                table: "gasolution_station_sale",
                type: "decimal(20,4)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "ValorRedondeo",
                schema: "raw",
                table: "gasolution_station_sale",
                type: "decimal(20,4)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BaseImponibleDocumento",
                schema: "raw",
                table: "gasolution_station_sale");

            migrationBuilder.DropColumn(
                name: "Cantidad",
                schema: "raw",
                table: "gasolution_station_sale");

            migrationBuilder.DropColumn(
                name: "Cara",
                schema: "raw",
                table: "gasolution_station_sale");

            migrationBuilder.DropColumn(
                name: "Cedula",
                schema: "raw",
                table: "gasolution_station_sale");

            migrationBuilder.DropColumn(
                name: "CodigoQr",
                schema: "raw",
                table: "gasolution_station_sale");

            migrationBuilder.DropColumn(
                name: "CodigoSunat",
                schema: "raw",
                table: "gasolution_station_sale");

            migrationBuilder.DropColumn(
                name: "Descuento",
                schema: "raw",
                table: "gasolution_station_sale");

            migrationBuilder.DropColumn(
                name: "Despachador",
                schema: "raw",
                table: "gasolution_station_sale");

            migrationBuilder.DropColumn(
                name: "EsDescuentoClienteCredito",
                schema: "raw",
                table: "gasolution_station_sale");

            migrationBuilder.DropColumn(
                name: "FechaHora",
                schema: "raw",
                table: "gasolution_station_sale");

            migrationBuilder.DropColumn(
                name: "FechaHoraDocumento",
                schema: "raw",
                table: "gasolution_station_sale");

            migrationBuilder.DropColumn(
                name: "FormaPago",
                schema: "raw",
                table: "gasolution_station_sale");

            migrationBuilder.DropColumn(
                name: "HoraFin",
                schema: "raw",
                table: "gasolution_station_sale");

            migrationBuilder.DropColumn(
                name: "HoraInicio",
                schema: "raw",
                table: "gasolution_station_sale");

            migrationBuilder.DropColumn(
                name: "IdVentaFacturacion",
                schema: "raw",
                table: "gasolution_station_sale");

            migrationBuilder.DropColumn(
                name: "ImpuestoDocumento",
                schema: "raw",
                table: "gasolution_station_sale");

            migrationBuilder.DropColumn(
                name: "LecturaFinal",
                schema: "raw",
                table: "gasolution_station_sale");

            migrationBuilder.DropColumn(
                name: "LecturaInicial",
                schema: "raw",
                table: "gasolution_station_sale");

            migrationBuilder.DropColumn(
                name: "Manguera",
                schema: "raw",
                table: "gasolution_station_sale");

            migrationBuilder.DropColumn(
                name: "NroConfirmacionCdc",
                schema: "raw",
                table: "gasolution_station_sale");

            migrationBuilder.DropColumn(
                name: "Numero",
                schema: "raw",
                table: "gasolution_station_sale");

            migrationBuilder.DropColumn(
                name: "NumeroTurno",
                schema: "raw",
                table: "gasolution_station_sale");

            migrationBuilder.DropColumn(
                name: "Placa",
                schema: "raw",
                table: "gasolution_station_sale");

            migrationBuilder.DropColumn(
                name: "Precio",
                schema: "raw",
                table: "gasolution_station_sale");

            migrationBuilder.DropColumn(
                name: "PrecioDocumento",
                schema: "raw",
                table: "gasolution_station_sale");

            migrationBuilder.DropColumn(
                name: "Producto",
                schema: "raw",
                table: "gasolution_station_sale");

            migrationBuilder.DropColumn(
                name: "RazonSocial",
                schema: "raw",
                table: "gasolution_station_sale");

            migrationBuilder.DropColumn(
                name: "Recaudo",
                schema: "raw",
                table: "gasolution_station_sale");

            migrationBuilder.DropColumn(
                name: "Recibo",
                schema: "raw",
                table: "gasolution_station_sale");

            migrationBuilder.DropColumn(
                name: "RedondeoDonacionDocumento",
                schema: "raw",
                table: "gasolution_station_sale");

            migrationBuilder.DropColumn(
                name: "Ruc",
                schema: "raw",
                table: "gasolution_station_sale");

            migrationBuilder.DropColumn(
                name: "Serie",
                schema: "raw",
                table: "gasolution_station_sale");

            migrationBuilder.DropColumn(
                name: "SourceDatabase",
                schema: "raw",
                table: "gasolution_station_sale");

            migrationBuilder.DropColumn(
                name: "SubtotalDocumento",
                schema: "raw",
                table: "gasolution_station_sale");

            migrationBuilder.DropColumn(
                name: "Surtidor",
                schema: "raw",
                table: "gasolution_station_sale");

            migrationBuilder.DropColumn(
                name: "TipoDocumento",
                schema: "raw",
                table: "gasolution_station_sale");

            migrationBuilder.DropColumn(
                name: "TipoRedondeo",
                schema: "raw",
                table: "gasolution_station_sale");

            migrationBuilder.DropColumn(
                name: "TotalPagarDocumento",
                schema: "raw",
                table: "gasolution_station_sale");

            migrationBuilder.DropColumn(
                name: "TramaEnviada",
                schema: "raw",
                table: "gasolution_station_sale");

            migrationBuilder.DropColumn(
                name: "Valor",
                schema: "raw",
                table: "gasolution_station_sale");

            migrationBuilder.DropColumn(
                name: "ValorCambioPrecio",
                schema: "raw",
                table: "gasolution_station_sale");

            migrationBuilder.DropColumn(
                name: "ValorRedondeo",
                schema: "raw",
                table: "gasolution_station_sale");

            migrationBuilder.AddColumn<string>(
                name: "Payload",
                schema: "raw",
                table: "gasolution_station_sale",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
