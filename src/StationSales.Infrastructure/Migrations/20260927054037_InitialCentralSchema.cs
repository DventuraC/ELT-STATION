using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StationSales.Infrastructure.Migrations
{
    public partial class InitialCentralSchema : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "etl");

            migrationBuilder.EnsureSchema(
                name: "raw");

            migrationBuilder.EnsureSchema(
                name: "core");

            migrationBuilder.CreateTable(
                name: "data_source",
                schema: "etl",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    StationCode = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Provider = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    DatabaseType = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Server = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Port = table.Column<int>(type: "int", nullable: false),
                    DatabaseName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Username = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SecretReference = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Environment = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_data_source", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "extraction_run",
                schema: "etl",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PipelineDataSourceId = table.Column<long>(type: "bigint", nullable: false),
                    DataSourceId = table.Column<long>(type: "bigint", nullable: false),
                    WindowFrom = table.Column<DateTime>(type: "datetime2", nullable: false),
                    WindowTo = table.Column<DateTime>(type: "datetime2", nullable: false),
                    StartedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FinishedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    RowsRead = table.Column<int>(type: "int", nullable: false),
                    RowsWritten = table.Column<int>(type: "int", nullable: false),
                    ErrorCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ErrorMessage = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_extraction_run", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "gasolution_station_sale",
                schema: "raw",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ExtractionRunId = table.Column<long>(type: "bigint", nullable: false),
                    DataSourceId = table.Column<long>(type: "bigint", nullable: false),
                    StationCode = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    SourceRecordId = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Payload = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SourceUpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ExtractedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_gasolution_station_sale", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "opencomb_station_sale",
                schema: "raw",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ExtractionRunId = table.Column<long>(type: "bigint", nullable: false),
                    DataSourceId = table.Column<long>(type: "bigint", nullable: false),
                    StationCode = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    SourceRecordId = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Payload = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SourceUpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ExtractedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_opencomb_station_sale", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "pipeline",
                schema: "etl",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PipelineType = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_pipeline", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "publish_run",
                schema: "etl",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Destination = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    StartedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FinishedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Checkpoint = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RowsProcessed = table.Column<int>(type: "int", nullable: false),
                    RowsInserted = table.Column<int>(type: "int", nullable: false),
                    RowsUpdated = table.Column<int>(type: "int", nullable: false),
                    ErrorMessage = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_publish_run", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "sale_document",
                schema: "core",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DocumentKeyHash = table.Column<byte[]>(type: "binary(32)", nullable: false),
                    DocumentHash = table.Column<byte[]>(type: "binary(32)", nullable: false),
                    SourceProvider = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    StationCode = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    LastSeenExtractionRunId = table.Column<long>(type: "bigint", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DeletedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_sale_document", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "transform_batch",
                schema: "etl",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PipelineId = table.Column<long>(type: "bigint", nullable: false),
                    StartedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FinishedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    DocumentsProcessed = table.Column<int>(type: "int", nullable: false),
                    DetailsProcessed = table.Column<int>(type: "int", nullable: false),
                    InsertedCount = table.Column<int>(type: "int", nullable: false),
                    UpdatedCount = table.Column<int>(type: "int", nullable: false),
                    UnchangedCount = table.Column<int>(type: "int", nullable: false),
                    SoftDeletedCount = table.Column<int>(type: "int", nullable: false),
                    ErrorMessage = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_transform_batch", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "pipeline_data_source",
                schema: "etl",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PipelineId = table.Column<long>(type: "bigint", nullable: false),
                    DataSourceId = table.Column<long>(type: "bigint", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastSuccessfulDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RecoveryDays = table.Column<int>(type: "int", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_pipeline_data_source", x => x.Id);
                    table.ForeignKey(
                        name: "FK_pipeline_data_source_data_source_DataSourceId",
                        column: x => x.DataSourceId,
                        principalSchema: "etl",
                        principalTable: "data_source",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_pipeline_data_source_pipeline_PipelineId",
                        column: x => x.PipelineId,
                        principalSchema: "etl",
                        principalTable: "pipeline",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "sale_document_detail",
                schema: "core",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SaleDocumentId = table.Column<long>(type: "bigint", nullable: false),
                    DetailKeyHash = table.Column<byte[]>(type: "binary(32)", nullable: false),
                    DetailHash = table.Column<byte[]>(type: "binary(32)", nullable: false),
                    LastSeenExtractionRunId = table.Column<long>(type: "bigint", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DeletedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_sale_document_detail", x => x.Id);
                    table.ForeignKey(
                        name: "FK_sale_document_detail_sale_document_SaleDocumentId",
                        column: x => x.SaleDocumentId,
                        principalSchema: "core",
                        principalTable: "sale_document",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "transform_batch_extraction",
                schema: "etl",
                columns: table => new
                {
                    TransformBatchId = table.Column<long>(type: "bigint", nullable: false),
                    ExtractionRunId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_transform_batch_extraction", x => new { x.TransformBatchId, x.ExtractionRunId });
                    table.ForeignKey(
                        name: "FK_transform_batch_extraction_transform_batch_TransformBatchId",
                        column: x => x.TransformBatchId,
                        principalSchema: "etl",
                        principalTable: "transform_batch",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_data_source_Code",
                schema: "etl",
                table: "data_source",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_extraction_run_DataSourceId_Status",
                schema: "etl",
                table: "extraction_run",
                columns: new[] { "DataSourceId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_gasolution_station_sale_DataSourceId_SourceRecordId",
                schema: "raw",
                table: "gasolution_station_sale",
                columns: new[] { "DataSourceId", "SourceRecordId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_gasolution_station_sale_ExtractionRunId",
                schema: "raw",
                table: "gasolution_station_sale",
                column: "ExtractionRunId");

            migrationBuilder.CreateIndex(
                name: "IX_opencomb_station_sale_DataSourceId_SourceRecordId",
                schema: "raw",
                table: "opencomb_station_sale",
                columns: new[] { "DataSourceId", "SourceRecordId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_opencomb_station_sale_ExtractionRunId",
                schema: "raw",
                table: "opencomb_station_sale",
                column: "ExtractionRunId");

            migrationBuilder.CreateIndex(
                name: "IX_pipeline_Code",
                schema: "etl",
                table: "pipeline",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_pipeline_data_source_DataSourceId",
                schema: "etl",
                table: "pipeline_data_source",
                column: "DataSourceId");

            migrationBuilder.CreateIndex(
                name: "IX_pipeline_data_source_PipelineId_DataSourceId",
                schema: "etl",
                table: "pipeline_data_source",
                columns: new[] { "PipelineId", "DataSourceId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_sale_document_DocumentKeyHash",
                schema: "core",
                table: "sale_document",
                column: "DocumentKeyHash",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_sale_document_detail_SaleDocumentId_DetailKeyHash",
                schema: "core",
                table: "sale_document_detail",
                columns: new[] { "SaleDocumentId", "DetailKeyHash" },
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "extraction_run",
                schema: "etl");

            migrationBuilder.DropTable(
                name: "gasolution_station_sale",
                schema: "raw");

            migrationBuilder.DropTable(
                name: "opencomb_station_sale",
                schema: "raw");

            migrationBuilder.DropTable(
                name: "pipeline_data_source",
                schema: "etl");

            migrationBuilder.DropTable(
                name: "publish_run",
                schema: "etl");

            migrationBuilder.DropTable(
                name: "sale_document_detail",
                schema: "core");

            migrationBuilder.DropTable(
                name: "transform_batch_extraction",
                schema: "etl");

            migrationBuilder.DropTable(
                name: "data_source",
                schema: "etl");

            migrationBuilder.DropTable(
                name: "pipeline",
                schema: "etl");

            migrationBuilder.DropTable(
                name: "sale_document",
                schema: "core");

            migrationBuilder.DropTable(
                name: "transform_batch",
                schema: "etl");
        }
    }
}
