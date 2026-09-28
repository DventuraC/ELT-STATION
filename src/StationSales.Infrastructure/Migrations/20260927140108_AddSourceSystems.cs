using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StationSales.Infrastructure.Migrations
{
    public partial class AddSourceSystems : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "SourceSystemId",
                schema: "etl",
                table: "data_source",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.CreateTable(
                name: "source_system",
                schema: "etl",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_source_system", x => x.Id);
                });

            migrationBuilder.InsertData(
                schema: "etl",
                table: "source_system",
                columns: new[] { "Id", "Code", "IsActive", "Name" },
                values: new object[] { 1L, "OPENCOMB", true, "OpenComb" });

            migrationBuilder.InsertData(
                schema: "etl",
                table: "source_system",
                columns: new[] { "Id", "Code", "IsActive", "Name" },
                values: new object[] { 2L, "GASOLUTION", true, "Gasolution" });

            migrationBuilder.CreateIndex(
                name: "IX_data_source_SourceSystemId",
                schema: "etl",
                table: "data_source",
                column: "SourceSystemId");

            migrationBuilder.CreateIndex(
                name: "IX_source_system_Code",
                schema: "etl",
                table: "source_system",
                column: "Code",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_data_source_source_system_SourceSystemId",
                schema: "etl",
                table: "data_source",
                column: "SourceSystemId",
                principalSchema: "etl",
                principalTable: "source_system",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_data_source_source_system_SourceSystemId",
                schema: "etl",
                table: "data_source");

            migrationBuilder.DropTable(
                name: "source_system",
                schema: "etl");

            migrationBuilder.DropIndex(
                name: "IX_data_source_SourceSystemId",
                schema: "etl",
                table: "data_source");

            migrationBuilder.DropColumn(
                name: "SourceSystemId",
                schema: "etl",
                table: "data_source");
        }
    }
}
