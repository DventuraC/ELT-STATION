using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StationSales.Infrastructure.Migrations
{
    public partial class AddStationCatalog : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "StationId",
                schema: "raw",
                table: "opencomb_station_sale",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "StationId",
                schema: "raw",
                table: "gasolution_station_sale",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "StationId",
                schema: "etl",
                table: "data_source",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.CreateTable(
                name: "station",
                schema: "etl",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    BusinessRuc = table.Column<string>(type: "nvarchar(11)", maxLength: 11, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_station", x => x.Id);
                });

            migrationBuilder.InsertData(
                schema: "etl",
                table: "station",
                columns: new[] { "Id", "BusinessRuc", "Code", "CreatedAtUtc", "IsActive", "Name", "UpdatedAtUtc" },
                values: new object[,]
                {
                    { 1L, "20506151547", "EDS_VENEZUELA", new DateTime(2026, 9, 27, 0, 0, 0, 0, DateTimeKind.Utc), true, "EDS VENEZUELA", new DateTime(2026, 9, 27, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 2L, "20506151547", "EDS_ICA", new DateTime(2026, 9, 27, 0, 0, 0, 0, DateTimeKind.Utc), true, "EDS ICA", new DateTime(2026, 9, 27, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 3L, "20506151547", "EDS_LA_MOLINA", new DateTime(2026, 9, 27, 0, 0, 0, 0, DateTimeKind.Utc), true, "EDS LA MOLINA", new DateTime(2026, 9, 27, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 4L, "20506151547", "EDS_VICTORIA", new DateTime(2026, 9, 27, 0, 0, 0, 0, DateTimeKind.Utc), true, "EDS VICTORIA", new DateTime(2026, 9, 27, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 5L, "20506151547", "EDS_VICTORIA_2", new DateTime(2026, 9, 27, 0, 0, 0, 0, DateTimeKind.Utc), true, "EDS VICTORIA 2", new DateTime(2026, 9, 27, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 6L, "20511706361", "EDS_BARRANCO", new DateTime(2026, 9, 27, 0, 0, 0, 0, DateTimeKind.Utc), true, "EDS BARRANCO", new DateTime(2026, 9, 27, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 7L, "20506151547", "EDS_MARINA", new DateTime(2026, 9, 27, 0, 0, 0, 0, DateTimeKind.Utc), true, "EDS MARINA", new DateTime(2026, 9, 27, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 8L, "20506151547", "EDS_NUEVO_CHIMBOTE", new DateTime(2026, 9, 27, 0, 0, 0, 0, DateTimeKind.Utc), true, "EDS NUEVO CHIMBOTE", new DateTime(2026, 9, 27, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 9L, "20506151547", "EDS_ARGENTINA", new DateTime(2026, 9, 27, 0, 0, 0, 0, DateTimeKind.Utc), true, "EDS ARGENTINA", new DateTime(2026, 9, 27, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 10L, "20506151547", "EDS_CALLAO", new DateTime(2026, 9, 27, 0, 0, 0, 0, DateTimeKind.Utc), true, "EDS CALLAO", new DateTime(2026, 9, 27, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 11L, "20506151547", "EDS_CHINCHA", new DateTime(2026, 9, 27, 0, 0, 0, 0, DateTimeKind.Utc), true, "EDS CHINCHA", new DateTime(2026, 9, 27, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 12L, "20506151547", "EDS_PUENTE_PIEDRA", new DateTime(2026, 9, 27, 0, 0, 0, 0, DateTimeKind.Utc), true, "EDS PUENTE PIEDRA", new DateTime(2026, 9, 27, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 13L, "20506151547", "EDS_LURIN", new DateTime(2026, 9, 27, 0, 0, 0, 0, DateTimeKind.Utc), true, "EDS LURIN", new DateTime(2026, 9, 27, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 14L, "20506151547", "EDS_ATE", new DateTime(2026, 9, 27, 0, 0, 0, 0, DateTimeKind.Utc), true, "EDS ATE", new DateTime(2026, 9, 27, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 15L, "20506151547", "EDS_CHIMBOTE_2", new DateTime(2026, 9, 27, 0, 0, 0, 0, DateTimeKind.Utc), true, "EDS CHIMBOTE 2", new DateTime(2026, 9, 27, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 16L, "20506151547", "EDS_TRUJILLO", new DateTime(2026, 9, 27, 0, 0, 0, 0, DateTimeKind.Utc), true, "EDS TRUJILLO", new DateTime(2026, 9, 27, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 17L, "20516752310", "EDS_TOMAS_VALLE", new DateTime(2026, 9, 27, 0, 0, 0, 0, DateTimeKind.Utc), true, "EDS TOMAS VALLE", new DateTime(2026, 9, 27, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 18L, "20516752310", "EDS_CAMPOY", new DateTime(2026, 9, 27, 0, 0, 0, 0, DateTimeKind.Utc), true, "EDS CAMPOY", new DateTime(2026, 9, 27, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 19L, "20506151547", "EDS_VENEZUELA_2", new DateTime(2026, 9, 27, 0, 0, 0, 0, DateTimeKind.Utc), true, "EDS VENEZUELA 2", new DateTime(2026, 9, 27, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 20L, "20506151547", "EDS_PAITA", new DateTime(2026, 9, 27, 0, 0, 0, 0, DateTimeKind.Utc), true, "EDS PAITA", new DateTime(2026, 9, 27, 0, 0, 0, 0, DateTimeKind.Utc) }
                });

            // Backfill existing connections and previously loaded RAW rows from the legacy station code.
            migrationBuilder.Sql(@"
                UPDATE d SET StationId = s.Id
                FROM [etl].[data_source] d
                INNER JOIN [etl].[station] s ON s.Code = d.StationCode;

                IF EXISTS (SELECT 1 FROM [etl].[data_source] WHERE StationId = 0)
                    THROW 51000, 'A data_source could not be mapped to etl.station.', 1;

                UPDATE r SET StationId = d.StationId
                FROM [raw].[opencomb_station_sale] r
                INNER JOIN [etl].[data_source] d ON d.Id = r.DataSourceId;

                UPDATE r SET StationId = d.StationId
                FROM [raw].[gasolution_station_sale] r
                INNER JOIN [etl].[data_source] d ON d.Id = r.DataSourceId;");

            migrationBuilder.CreateIndex(
                name: "IX_data_source_StationId",
                schema: "etl",
                table: "data_source",
                column: "StationId");

            migrationBuilder.CreateIndex(
                name: "IX_station_Code",
                schema: "etl",
                table: "station",
                column: "Code",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_data_source_station_StationId",
                schema: "etl",
                table: "data_source",
                column: "StationId",
                principalSchema: "etl",
                principalTable: "station",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_data_source_station_StationId",
                schema: "etl",
                table: "data_source");

            migrationBuilder.DropTable(
                name: "station",
                schema: "etl");

            migrationBuilder.DropIndex(
                name: "IX_data_source_StationId",
                schema: "etl",
                table: "data_source");

            migrationBuilder.DropColumn(
                name: "StationId",
                schema: "raw",
                table: "opencomb_station_sale");

            migrationBuilder.DropColumn(
                name: "StationId",
                schema: "raw",
                table: "gasolution_station_sale");

            migrationBuilder.DropColumn(
                name: "StationId",
                schema: "etl",
                table: "data_source");
        }
    }
}
